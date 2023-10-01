namespace FlexInject.Enums;

/// <summary>
/// Specifies the lifetime of a service in the <see cref="FlexInjectContainer"/>.
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// Specifies that a new instance of the service will be created every time it is requested.
    /// </summary>
    Transient,

    /// <summary>
    /// Specifies that a single instance of the service will be created and shared within the scope of a single request.
    /// </summary>
    Scoped,

    /// <summary>
    /// Specifies that a single instance of the service will be created and shared across multiple requests.
    /// </summary>
    Singleton
}
