using FlexInject.Enums;

namespace FlexInject;

/// <summary>
/// Describes a service with its implementation type, lifetime, name, and tag. 
/// This metadata is used by the <see cref="FlexInjectContainer"/> to register and resolve services.
/// </summary>
public class ServiceDescriptor
{
    /// <summary>
    /// Gets the type of the service being described.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the type that implements the service being described.
    /// </summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// Gets the <see cref="ServiceLifetime"/> of the service being described.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Gets the name associated with the service being described.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the tag associated with the service being described.
    /// </summary>
    public string Tag { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDescriptor"/> class with the specified service type, 
    /// implementation type, lifetime, name, and tag.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="implementationType">The type that implements the service.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    /// <param name="name">The name associated with the service.</param>
    /// <param name="tag">The tag associated with the service.</param>
    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name = null, string tag = null)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
        Name = name;
        Tag = tag;
    }
}