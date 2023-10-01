namespace FlexInject;

public class ServiceDescriptor
{
    public Type ServiceType { get; }

    public Type ImplementationType { get; }

    public ServiceLifetime Lifetime { get; }

    public string Name { get; init; }

    public string Tag { get; init; }

    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name = null, string tag = null)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
        Name = name;
        Tag = tag;
    }
}

public enum ServiceLifetime
{
    Singleton,
    Scoped,
    Transient
}

