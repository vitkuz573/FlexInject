using FlexInject.Abstractions;
using FlexInject.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace FlexInject;

public class FlexInjectContainer : IDisposable
{
    private readonly ConcurrentDictionary<(Type, string, string), Type> _typeMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), Type> _transientMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), Type> _scopedMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), object> _singletonInstances = new();
    private readonly AsyncLocal<ConcurrentDictionary<(Type, string, string), object>> _scopedInstances = new();
    private readonly AsyncLocal<Stack<Type>> _resolveStack = new();
    private readonly List<IResolvePolicy> _policies = new();

    public void Register<TInterface, TImplementation>(string name = null, string tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        var key = (typeof(TInterface), name ?? "default", tag ?? "default");

        if (_typeMapping.ContainsKey(key))
        {
            throw new InvalidOperationException($"Type {typeof(TInterface).FullName} with name {name ?? "default"} and tag {tag ?? "default"} is already registered.");
        }

        _typeMapping[key] = typeof(TImplementation);
    }

    public void RegisterTransient<TInterface, TImplementation>(string name = null, string tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        var key = (typeof(TInterface), name ?? "default", tag ?? "default");

        if (_transientMapping.ContainsKey(key))
        {
            throw new InvalidOperationException($"Transient type {typeof(TInterface).FullName} with name {name ?? "default"} and tag {tag ?? "default"} is already registered.");
        }

        _transientMapping[key] = typeof(TImplementation);
    }

    public void RegisterScoped<TInterface, TImplementation>(string name = null, string tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        var key = (typeof(TInterface), name ?? "default", tag ?? "default");

        if (_scopedMapping.ContainsKey(key))
        {
            throw new InvalidOperationException($"Scoped type {typeof(TInterface).FullName} with name {name ?? "default"} and tag {tag ?? "default"} is already registered.");
        }

        _scopedMapping[key] = typeof(TImplementation);
    }

    public void RegisterSingleton<TInterface, TImplementation>(string name = null, string tag = null) where TImplementation : TInterface, new()
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        var key = (typeof(TInterface), name ?? "default", tag ?? "default");

        if (_singletonInstances.ContainsKey(key))
        {
            throw new InvalidOperationException($"Singleton instance of type {typeof(TInterface).FullName} with name {name ?? "default"} and tag {tag ?? "default"} is already registered.");
        }

        TImplementation instance = new();

        _singletonInstances[key] = instance;
    }

    private static void ValidateTypeCompatibility<TInterface, TImplementation>() where TImplementation : TInterface
    {
        if (!typeof(TInterface).IsAssignableFrom(typeof(TImplementation)))
        {
            throw new InvalidOperationException($"Type {typeof(TImplementation).FullName} does not implement {typeof(TInterface).FullName}.");
        }
    }

    public TInterface Resolve<TInterface>(string name = null, string tag = null)
    {
        return (TInterface)Resolve(typeof(TInterface), name, tag);
    }

    public object Resolve(Type type, string name = null, string tag = null)
    {
        try
        {
            var scopedInstances = _scopedInstances.Value;
            var resolveStack = _resolveStack.Value ??= new Stack<Type>();

            if (resolveStack.Contains(type))
            {
                throw new InvalidOperationException($"Detected a cyclic dependency: {string.Join(" -> ", resolveStack.Reverse().Select(t => t.Name)) + " -> " + type.Name}");
            }

            resolveStack.Push(type);

            if (scopedInstances != null && scopedInstances.TryGetValue((type, name ?? "default", tag ?? "default"), out object instance))
            {
                return instance;
            }

            if (_singletonInstances.TryGetValue((type, name ?? "default", tag ?? "default"), out instance))
            {
                return instance;
            }

            foreach (var policy in _policies)
            {
                var policyInstance = policy.Resolve(this, type, name, tag);

                if (policyInstance != null)
                {
                    return policyInstance;
                }
            }

            if (_typeMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out Type implementationType))
            {
                var createdInstance = CreateInstance(implementationType);
                return createdInstance;
            }

            if (_transientMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out implementationType))
            {
                return CreateInstance(implementationType);
            }

            if (scopedInstances != null && _scopedMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out implementationType))
            {
                if (!scopedInstances.TryGetValue((type, name ?? "default", tag ?? "default"), out instance))
                {
                    instance = CreateInstance(implementationType);
                    scopedInstances.TryAdd((type, name ?? "default", tag ?? "default"), instance);
                }

                return instance;
            }

            throw new InvalidOperationException($"Type {type.FullName} with name {name ?? "default"} and tag {tag ?? "default"} is not registered.");
        }
        finally
        {
            if (_resolveStack.Value?.Count > 0)
            {
                _resolveStack.Value.Pop();
            }
        }
    }

    private object CreateInstance(Type implementationType)
    {
        var constructor = implementationType.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var parameterInstances = parameters.Select(p => Resolve(p.ParameterType)).ToArray();
        var instance = Activator.CreateInstance(implementationType, parameterInstances);

        foreach (var field in implementationType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.GetCustomAttribute<InjectAttribute>() != null))
        {
            var attr = field.GetCustomAttribute<InjectAttribute>();
            field.SetValue(instance, Resolve(field.FieldType, attr.Name, attr.Tag));
        }

        foreach (var property in implementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<InjectAttribute>() != null))
        {
            var attr = property.GetCustomAttribute<InjectAttribute>();
            property.SetValue(instance, Resolve(property.PropertyType, attr.Name, attr.Tag));
        }

        (instance as IInitialize)?.Initialize();

        return instance;
    }

    public void AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);
    }

    public IDisposable CreateScope()
    {
        _scopedInstances.Value = new ConcurrentDictionary<(Type, string, string), object>();

        return new Disposer(() => _scopedInstances.Value = null);
    }

    public void Dispose()
    {
        foreach (var instance in _singletonInstances.Values.Concat(_scopedInstances.Value?.Values ?? Enumerable.Empty<object>()))
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        GC.SuppressFinalize(this);
    }

    private class Disposer : IDisposable
    {
        private readonly Action _dispose;

        public Disposer(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose() => _dispose();
    }
}

public interface IResolvePolicy
{
    object Resolve(FlexInjectContainer container, Type type, string name, string tag);
}
