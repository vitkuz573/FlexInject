using FlexInject.Abstractions;
using FlexInject.Enums;
using FlexInject.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace FlexInject;

/// <summary>
/// The main dependency injection container.
/// Provides registration with various lifetimes, custom resolution policies, and scoping.
/// Only constructor injection is supported.
/// </summary>
public class FlexInjectContainer : IDisposable
{
    // Holds all registrations keyed by InjectionKey.
    private readonly ConcurrentDictionary<InjectionKey, Registration> _registrations = [];
    // List of custom resolution policies.
    private readonly List<IResolvePolicy> _policies = [];

    // Cache for the selected constructor for each type to improve performance.
    private readonly ConcurrentDictionary<Type, ConstructorInfo> _constructorCache = [];

    // Used to detect circular dependencies.
    private readonly AsyncLocal<Stack<Type>> _resolveStack = new();
    // Holds the current active scope (if any).
    private readonly AsyncLocal<FlexInjectScope?> _currentScope = new();

    /// <summary>
    /// Gets the current active scope.
    /// </summary>
    internal FlexInjectScope? CurrentScope => _currentScope.Value;

    #region Registration

    /// <summary>
    /// Registers a transient dependency.
    /// </summary>
    /// <typeparam name="TInterface">The service interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="name">An optional name identifier.</param>
    /// <param name="tag">An optional tag identifier.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the type is already registered.
    /// </exception>
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

    /// <summary>
    /// Registers a transient dependency.
    /// </summary>
    public void RegisterTransient<TInterface, TImplementation>(string? name = null, string? tag = null) where TImplementation : TInterface
    {
        Register<TInterface, TImplementation>(name, tag);
    }

    /// <summary>
    /// Registers a scoped dependency.
    /// </summary>
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

    /// <summary>
    /// Registers a singleton dependency.
    /// </summary>
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

    /// <summary>
    /// Resolves an instance of the specified service type.
    /// </summary>
    /// <typeparam name="TInterface">The service interface type.</typeparam>
    /// <param name="name">An optional name identifier.</param>
    /// <param name="tag">An optional tag identifier.</param>
    /// <returns>An instance of the requested type.</returns>
    public TInterface Resolve<TInterface>(string? name = null, string? tag = null)
    {
        return (TInterface)Resolve(typeof(TInterface), name, tag);
    }

    /// <summary>
    /// Resolves an instance of the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <param name="name">An optional name identifier.</param>
    /// <param name="tag">An optional tag identifier.</param>
    /// <returns>An instance of the requested type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a circular dependency is detected or the type is not registered.
    /// </exception>
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
            // Check custom resolution policies first.
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
    /// Creates a new instance of the specified implementation type using constructor injection.
    /// </summary>
    /// <param name="implementationType">The implementation type to instantiate.</param>
    /// <returns>A new instance of the implementation type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no public constructor is found or instance creation fails.
    /// </exception>
    internal object CreateInstance(Type implementationType)
    {
        // Attempt to get the constructor from cache or compute and cache it.
        var ctor = _constructorCache.GetOrAdd(implementationType, type =>
            type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault() ?? throw new InvalidOperationException($"No public constructors found for {type.FullName}"));

        var parameters = ctor.GetParameters();
        var parameterInstances = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameterInstances[i] = Resolve(parameters[i].ParameterType);
        }

        // Create instance using constructor injection.
        var instance = Activator.CreateInstance(implementationType, parameterInstances) ?? throw new InvalidOperationException($"Failed to create an instance of {implementationType.FullName}");

        if (instance is IInitialize initializer)
        {
            initializer.Initialize();
        }

        return instance;
    }

    #endregion

    #region Scoping

    /// <summary>
    /// Creates a new scope. Scoped dependencies are cached within the scope.
    /// When the scope is disposed, the cached objects are also disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that ends the scope when disposed.</returns>
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

    /// <summary>
    /// Adds a custom resolution policy.
    /// </summary>
    /// <param name="policy">The custom policy to add.</param>
    public void AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);
    }

    #endregion

    #region IDisposable Support

    /// <summary>
    /// Disposes the container and any disposable singleton or scoped instances.
    /// </summary>
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
