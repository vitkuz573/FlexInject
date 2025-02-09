using FlexInject.Abstractions;
using FlexInject.Attributes;
using FlexInject.Enums;
using FlexInject.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace FlexInject;

/// <summary>
/// The main DI container. It supports registration with various lifetimes,
/// custom resolution policies, property/field injection, and scoping.
/// </summary>
public class FlexInjectContainer : IDisposable
{
    private readonly ConcurrentDictionary<InjectionKey, Registration> _registrations = [];
    private readonly List<IResolvePolicy> _policies = [];

    private readonly AsyncLocal<Stack<Type>> _resolveStack = new();
    private readonly AsyncLocal<FlexInjectScope?> _currentScope = new();

    public FlexInjectScope? CurrentScope => _currentScope.Value;

    #region Registration

    public void Register<TInterface, TImplementation>(string? name = null, string? tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        
        var key = new InjectionKey(typeof(TInterface), name, tag);
        var reg = new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Transient);
        
        if (!_registrations.TryAdd(key, reg))
        {
            throw new InvalidOperationException($"Type {typeof(TInterface).FullName} with name '{name ?? "default"}' and tag '{tag ?? "default"}' is already registered.");
        }
    }

    public void RegisterTransient<TInterface, TImplementation>(string? name = null, string? tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        
        var key = new InjectionKey(typeof(TInterface), name, tag);
        var reg = new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Transient);
        
        if (!_registrations.TryAdd(key, reg))
        {
            throw new InvalidOperationException($"Transient type {typeof(TInterface).FullName} with name '{name ?? "default"}' and tag '{tag ?? "default"}' is already registered.");
        }
    }

    public void RegisterScoped<TInterface, TImplementation>(string? name = null, string? tag = null) where TImplementation : TInterface
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        
        var key = new InjectionKey(typeof(TInterface), name, tag);
        var reg = new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Scoped);
        
        if (!_registrations.TryAdd(key, reg))
        {
            throw new InvalidOperationException($"Scoped type {typeof(TInterface).FullName} with name '{name ?? "default"}' and tag '{tag ?? "default"}' is already registered.");
        }
    }

    public void RegisterSingleton<TInterface, TImplementation>(string? name = null, string? tag = null) where TImplementation : TInterface, new()
    {
        ValidateTypeCompatibility<TInterface, TImplementation>();
        
        var key = new InjectionKey(typeof(TInterface), name, tag);
        var reg = new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Singleton);
        
        if (!_registrations.TryAdd(key, reg))
        {
            throw new InvalidOperationException($"Singleton type {typeof(TInterface).FullName} with name '{name ?? "default"}' and tag '{tag ?? "default"}' is already registered.");
        }
    }

    private static void ValidateTypeCompatibility<TInterface, TImplementation>() where TImplementation : TInterface
    {
        if (!typeof(TInterface).IsAssignableFrom(typeof(TImplementation)))
        {
            throw new InvalidOperationException($"Type {typeof(TImplementation).FullName} does not implement {typeof(TInterface).FullName}.");
        }
    }

    #endregion

    #region Dependency Resolution

    public TInterface Resolve<TInterface>(string? name = null, string? tag = null)
    {
        return (TInterface)Resolve(typeof(TInterface), name, tag);
    }

    public object Resolve(Type serviceType, string? name = null, string? tag = null)
    {
        var key = new InjectionKey(serviceType, name, tag);

        var stack = _resolveStack.Value ??= new Stack<Type>();
        
        if (stack.Contains(serviceType))
        {
            throw new InvalidOperationException($"Circular dependency detected for type {serviceType.FullName}");
        }

        stack.Push(serviceType);
        
        try
        {
            foreach (var policy in _policies)
            {
                var result = policy.Resolve(this, serviceType, name, tag);
                
                if (result != null)
                {
                    return result;
                }
            }

            if (_registrations.TryGetValue(key, out var registration))
            {
                return registration.GetInstance(this, key);
            }
            else
            {
                throw new InvalidOperationException($"Type {serviceType.FullName} with name '{name ?? "default"}' and tag '{tag ?? "default"}' is not registered.");
            }
        }
        finally
        {
            stack.Pop();
        }
    }

    /// <summary>
    /// Creates a new instance of the specified implementation type,
    /// performing constructor injection as well as property and field injection.
    /// </summary>
    internal object CreateInstance(Type implementationType)
    {
        var ctor = implementationType.GetConstructors()
                                     .OrderByDescending(c => c.GetParameters().Length)
                                     .FirstOrDefault() ?? throw new InvalidOperationException($"No public constructors found for {implementationType.FullName}");
        
        var parameters = ctor.GetParameters();
        var parameterInstances = new object?[parameters.Length];
        
        for (int i = 0; i < parameters.Length; i++)
        {
            parameterInstances[i] = Resolve(parameters[i].ParameterType);
        }
        
        var instance = Activator.CreateInstance(implementationType, parameterInstances) ?? throw new InvalidOperationException($"Failed to create an instance of {implementationType.FullName}");

        InjectDependencies(instance, implementationType);

        if (instance is IInitialize initializer)
        {
            initializer.Initialize();
        }

        return instance;
    }

    private void InjectDependencies(object instance, Type implementationType)
    {
        var fields = implementationType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Where(f => f.IsDefined(typeof(InjectAttribute), inherit: true));
        
        foreach (var field in fields)
        {
            var attr = field.GetCustomAttribute<InjectAttribute>(inherit: true);
            
            if (attr != null)
            {
                var dependency = Resolve(field.FieldType, attr.Name, attr.Tag);
                field.SetValue(instance, dependency);
            }
        }

        var properties = implementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(p => p.IsDefined(typeof(InjectAttribute), inherit: true));
        
        foreach (var property in properties)
        {
            var attr = property.GetCustomAttribute<InjectAttribute>(inherit: true);
            
            if (attr != null && property.CanWrite)
            {
                var dependency = Resolve(property.PropertyType, attr.Name, attr.Tag);
                property.SetValue(instance, dependency);
            }
        }
    }

    #endregion

    #region Scoping

    /// <summary>
    /// Creates a new scope. Within a scope, all Scoped dependencies are cached.
    /// When the scope is disposed, the cached objects are disposed.
    /// </summary>
    public IDisposable CreateScope()
    {
        var scope = new FlexInjectScope();
        
        _currentScope.Value = scope;

        return new ScopeWrapper(this, scope);
    }

    private void EndScope(FlexInjectScope scope)
    {
        if (_currentScope.Value == scope)
        {
            _currentScope.Value = null;
        }
    }

    private class ScopeWrapper(FlexInjectContainer container, FlexInjectScope scope) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                scope.Dispose();
                container.EndScope(scope);

                _disposed = true;
            }
        }
    }

    #endregion

    #region Resolution Policies

    public void AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);
    }

    #endregion

    #region IDisposable Support

    public void Dispose()
    {
        foreach (var reg in _registrations.Values)
        {
            if (reg.Lifetime == Lifetime.Singleton)
            {
                var instance = reg.GetInstance(this, new InjectionKey(reg.ServiceType, null, null));
                
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        _currentScope.Value?.Dispose();
    }

    #endregion
}
