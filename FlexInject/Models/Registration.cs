using FlexInject.Enums;

namespace FlexInject.Models;

/// <summary>
/// Encapsulates a dependency registration. It stores the service type, its implementation,
/// lifetime, and an optional factory for custom instantiation.
/// </summary>
public class Registration(Type serviceType, Type implementationType, Lifetime lifetime, Func<FlexInjectContainer, object>? factory = null)
{
    public Type ServiceType { get; } = serviceType;
    
    public Type ImplementationType { get; } = implementationType;
    
    public Lifetime Lifetime { get; } = lifetime;
    
    public Func<FlexInjectContainer, object>? Factory { get; } = factory;

    private object? _singletonInstance;

    private readonly object _lock = new();

    /// <summary>
    /// Retrieves an instance of the dependency based on its lifetime.
    /// </summary>
    public object GetInstance(FlexInjectContainer container, InjectionKey key)
    {
        switch (Lifetime)
        {
            case Lifetime.Singleton:
                if (_singletonInstance == null)
                {
                    lock (_lock)
                    {
                        _singletonInstance ??= CreateInstance(container);
                    }
                }
                return _singletonInstance;
            case Lifetime.Scoped:
                var scope = container.CurrentScope ?? throw new InvalidOperationException("Attempted to resolve a scoped dependency without an active scope.");
                
                return scope.GetOrAdd(key, () => CreateInstance(container));
            case Lifetime.Transient:
            default:
                return CreateInstance(container);
        }
    }

    private object CreateInstance(FlexInjectContainer container)
    {
        if (Factory != null)
        {
            return Factory(container);
        }

        return container.CreateInstance(ImplementationType);
    }
}
