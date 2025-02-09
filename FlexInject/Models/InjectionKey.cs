namespace FlexInject.Models;

/// <summary>
/// Represents a key for dependency registrations. It encapsulates the service type,
/// a name, and a tag.
/// </summary>
public class InjectionKey(Type serviceType, string? name, string? tag) : IEquatable<InjectionKey>
{
    public Type ServiceType { get; } = serviceType;

    public string Name { get; } = name ?? "default";
    
    public string Tag { get; } = tag ?? "default";

    public override bool Equals(object? obj) => Equals(obj as InjectionKey);

    public bool Equals(InjectionKey? other)
    {
        if (other is null)
        {
            return false;
        }

        return ServiceType == other.ServiceType &&
               Name == other.Name &&
               Tag == other.Tag;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ServiceType, Name, Tag);
    }
}
