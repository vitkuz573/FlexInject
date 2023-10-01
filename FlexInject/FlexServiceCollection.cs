using FlexInject.Abstractions;

namespace FlexInject;

public class FlexServiceCollection : IFlexServiceCollection
{
    private readonly List<ServiceDescriptor> _services = new();
    private readonly List<IResolvePolicy> _policies = new();

    public IFlexServiceCollection AddTransient<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), ServiceLifetime.Transient);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, name, tag));
        
        return this;
    }

    public IFlexServiceCollection AddScoped<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), ServiceLifetime.Scoped);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, name, tag));
        
        return this;
    }

    public IFlexServiceCollection AddSingleton<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService
    {
        CheckForExistingRegistration(typeof(TService), ServiceLifetime.Singleton);
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, name, tag));
        
        return this;
    }

    private void CheckForExistingRegistration(Type serviceType, ServiceLifetime lifetime, string name = null, string tag = null)
    {
        if (_services.Any(s => s.ServiceType == serviceType && s.Lifetime == lifetime && s.Name == name && s.Tag == tag))
        {
            throw new InvalidOperationException($"{serviceType.FullName} has already been registered with {lifetime} lifetime, name {name ?? "default"} and tag {tag ?? "default"}.");
        }
    }

    public IFlexServiceCollection AddPolicy(IResolvePolicy policy)
    {
        _policies.Add(policy);

        return this;
    }

    public FlexInjectContainer BuildServiceProvider()
    {
        var container = new FlexInjectContainer(_services);

        foreach (var policy in _policies)
        {
            container.AddPolicy(policy);
        }

        return container;
    }
}
