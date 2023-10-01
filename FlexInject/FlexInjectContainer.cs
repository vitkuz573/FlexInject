using FlexInject.Abstractions;
using FlexInject.Enums;
using System.Collections.Concurrent;

namespace FlexInject;

public class FlexInjectContainer : IFlexServiceProvider, IDisposable
{
    private readonly ConcurrentDictionary<(Type, string, string), Type> _typeMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), Type> _transientMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), Type> _scopedMapping = new();
    private readonly ConcurrentDictionary<(Type, string, string), object> _singletonInstances = new();
    private readonly AsyncLocal<ConcurrentDictionary<(Type, string, string), object>> _scopedInstances = new();
    private readonly AsyncLocal<Stack<Type>> _resolveStack = new();
    private readonly List<IResolvePolicy> _policies = new();

    public FlexInjectContainer(IEnumerable<ServiceDescriptor> services)
    {
        foreach (var service in services)
        {
            var key = (service.ServiceType, service.Name ?? "default", service.Tag ?? "default");

            switch (service.Lifetime)
            {
                case ServiceLifetime.Transient:
                    _transientMapping[key] = service.ImplementationType;
                    break;
                case ServiceLifetime.Scoped:
                    _scopedMapping[key] = service.ImplementationType;
                    break;
                case ServiceLifetime.Singleton:
                    var instance = Activator.CreateInstance(service.ImplementationType);
                    _singletonInstances[key] = instance;
                    break;
            }
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

            if (_typeMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out Type implementationType)
                || _transientMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out implementationType)
                || (scopedInstances != null && _scopedMapping.TryGetValue((type, name ?? "default", tag ?? "default"), out implementationType)))
            {
                var createdInstance = CreateInstance(implementationType);

                if (scopedInstances != null && _scopedMapping.ContainsKey((type, name ?? "default", tag ?? "default")))
                {
                    scopedInstances.TryAdd((type, name ?? "default", tag ?? "default"), createdInstance);
                }

                return createdInstance;
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

    public object GetService(Type serviceType, string name = null, string tag = null)
    {
        return Resolve(serviceType, name, tag);
    }

    public T GetService<T>(string name = null, string tag = null)
    {
        return (T)GetService(typeof(T), name, tag);
    }

    private object CreateInstance(Type implementationType)
    {
        var constructor = implementationType.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var parameterInstances = parameters.Select(p => Resolve(p.ParameterType)).ToArray();
        var instance = Activator.CreateInstance(implementationType, parameterInstances);

        (instance as IInitialize)?.Initialize();

        return instance;
    }

    public void AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);
    }

    public IFlexServiceScope CreateScope()
    {
        var scope = new FlexServiceScope(this);

        return scope;
    }

    public void InitializeScopedInstances()
    {
        _scopedInstances.Value = new ConcurrentDictionary<(Type, string, string), object>();
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
