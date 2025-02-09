namespace FlexInject.Abstractions;

/// <summary>
/// Interface for custom resolution policies.
/// </summary>
public interface IResolvePolicy
{
    object? Resolve(FlexInjectContainer container, Type type, string? name, string? tag);
}
