namespace FlexInject.Attributes;

/// <summary>
/// Specifies that a dependency should be injected into a property or field when an instance 
/// of the target type is resolved using the <see cref="FlexInjectContainer"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name associated with the service to resolve.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the tag associated with the service to resolve.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectAttribute"/> class with the specified name and tag.
    /// </summary>
    /// <param name="name">The name associated with the service to resolve.</param>
    /// <param name="tag">The tag associated with the service to resolve.</param>
    public InjectAttribute(string name = null, string tag = null)
    {
        Name = name;
        Tag = tag;
    }
}
