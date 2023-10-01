namespace FlexInject.Abstractions;

/// <summary>
/// Represents a mechanism for retrieving services.
/// </summary>
public interface IFlexServiceProvider : IDisposable
{
    /// <summary>
    /// Gets the service of type <paramref name="serviceType"/> from the service provider.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <param name="name">The name associated with the service. (Optional)</param>
    /// <param name="tag">The tag associated with the service. (Optional)</param>
    /// <returns>A service object of type <paramref name="serviceType"/> or null if there is no such service.</returns>
    object GetService(Type serviceType, string name = null, string tag = null);

    /// <summary>
    /// Gets the service of type <typeparamref name="T"/> from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="name">The name associated with the service. (Optional)</param>
    /// <param name="tag">The tag associated with the service. (Optional)</param>
    /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
    T GetService<T>(string name = null, string tag = null);

    /// <summary>
    /// Creates a new scope for retrieving services.
    /// </summary>
    /// <returns>An <see cref="IFlexServiceScope"/> which can be used to resolve scoped services.</returns>
    IFlexServiceScope CreateScope();
}
