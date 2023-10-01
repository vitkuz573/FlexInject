namespace FlexInject.Abstractions;

/// <summary>
/// Represents a collection of services to be configured and built into an <see cref="IFlexServiceProvider"/>.
/// </summary>
public interface IFlexServiceCollection
{
    /// <summary>
    /// Adds a transient service of the type specified in <paramref name="TService"/> with an implementation
    /// of the type specified in <paramref name="TImplementation"/> to the specified service collection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="name">The name to associate with the service. (Optional)</param>
    /// <param name="tag">The tag to associate with the service. (Optional)</param>
    /// <returns>The <see cref="IFlexServiceCollection"/> so that additional calls can be chained.</returns>
    IFlexServiceCollection AddTransient<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    /// <summary>
    /// Adds a scoped service of the type specified in <paramref name="TService"/> with an implementation
    /// of the type specified in <paramref name="TImplementation"/> to the specified service collection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="name">The name to associate with the service. (Optional)</param>
    /// <param name="tag">The tag to associate with the service. (Optional)</param>
    /// <returns>The <see cref="IFlexServiceCollection"/> so that additional calls can be chained.</returns>
    IFlexServiceCollection AddScoped<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    /// <summary>
    /// Adds a singleton service of the type specified in <paramref name="TService"/> with an implementation
    /// of the type specified in <paramref name="TImplementation"/> to the specified service collection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="name">The name to associate with the service. (Optional)</param>
    /// <param name="tag">The tag to associate with the service. (Optional)</param>
    /// <returns>The <see cref="IFlexServiceCollection"/> so that additional calls can be chained.</returns>
    IFlexServiceCollection AddSingleton<TService, TImplementation>(string name = null, string tag = null) where TImplementation : TService;

    /// <summary>
    /// Adds a policy used to resolve services.
    /// </summary>
    /// <param name="policy">The policy to add to the service collection.</param>
    /// <returns>The <see cref="IFlexServiceCollection"/> so that additional calls can be chained.</returns>
    IFlexServiceCollection AddPolicy(IResolvePolicy policy);

    /// <summary>
    /// Builds the service provider from the service collection.
    /// </summary>
    /// <returns>An instance of <see cref="IFlexServiceProvider"/> that provides access to the services of this collection.</returns>
    IFlexServiceProvider BuildServiceProvider();
}
