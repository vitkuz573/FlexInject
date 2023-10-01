using FlexInject.Abstractions;

namespace FlexInject;

public class FlexServiceCollection : IFlexServiceCollection
{
    private readonly List<ServiceDescriptor> _services = new();
    private readonly List<IResolvePolicy> _policies = new();

    public IFlexServiceCollection AddTransient<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, name, tag);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, name, tag));

        return this;
    }

    public IFlexServiceCollection AddScoped<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, name, tag);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, name, tag));

        return this;
    }

    public IFlexServiceCollection AddSingleton<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, name, tag);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, name, tag));

        return this;
    }

    private void CheckForExistingRegistration(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name = null, string tag = null)
    {
        if (_services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementationType && s.Lifetime == lifetime && s.Name == name && s.Tag == tag))
        {
            throw new InvalidOperationException($"{serviceType.FullName} with implementation {implementationType.FullName} has already been registered with {lifetime} lifetime, name {name ?? "default"} and tag {tag ?? "default"}.");
        }
    }

    public IFlexServiceCollection AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);

        return this;
    }

    public IFlexServiceProvider BuildServiceProvider()
    {
        var container = new FlexInjectContainer(_services);

        foreach (var policy in _policies)
        {
            container.AddPolicy(policy);
        }

        return container;
    }
}
