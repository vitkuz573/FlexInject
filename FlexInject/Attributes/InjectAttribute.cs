namespace FlexInject.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute(string? name = null, string? tag = null) : Attribute
{
    public string? Name { get; set; } = name;

    public string? Tag { get; set; } = tag;
}
