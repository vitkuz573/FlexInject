namespace FlexInject.Abstractions;

/// <summary>
/// Defines a policy that can be used to resolve services. A resolve policy can provide
/// an alternative way to create instances of services, allowing for custom logic or
/// decision-making in the resolution process.
/// </summary>
public interface IResolvePolicy
{
    /// <summary>
    /// Attempts to resolve a service of the given type with the provided name and tag, using
    /// the specified container. If this policy does not apply or cannot resolve the service,
    /// it should return null, allowing other policies or the container itself to attempt resolution.
    /// </summary>
    /// <param name="container">The <see cref="FlexInjectContainer"/> used for resolution.</param>
    /// <param name="type">The <see cref="Type"/> of service to resolve.</param>
    /// <param name="name">The name used to register the service, if any.</param>
    /// <param name="tag">The tag used to register the service, if any.</param>
    /// <returns>An instance of the service if it can be resolved by this policy; otherwise, null.</returns>
    object Resolve(FlexInjectContainer container, Type type, string name, string tag);
}
