namespace FlexInject.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public string Name { get; }

    public string Tag { get; }

    public InjectAttribute(string name = null, string tag = null)
    {
        Name = name;
        Tag = tag;
    }
}