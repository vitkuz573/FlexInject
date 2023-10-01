namespace FlexInject.Abstractions;

/// <summary>
/// Represents a scope for retrieving services, allowing for proper management and disposal of
/// service instances, particularly for those services registered with a Scoped lifetime.
/// </summary>
public interface IFlexServiceScope : IDisposable
{
    /// <summary>
    /// Gets the service provider used to resolve dependencies from this scope.
    /// </summary>
    IFlexServiceProvider ServiceProvider { get; }
}
