namespace FlexInject.Abstractions;

public interface IFlexServiceCollection
{
    IFlexServiceCollection AddTransient<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    IFlexServiceCollection AddScoped<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    IFlexServiceCollection AddSingleton<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    IFlexServiceCollection AddPolicy(IResolvePolicy policy);

    IFlexServiceProvider BuildServiceProvider();
}
