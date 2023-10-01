namespace FlexInject.Attributes;

/// <summary>
/// Specifies that a dependency should be injected into a property or field when an instance 
/// of the target type is resolved using the <see cref="FlexInjectContainer"/>.
/// </summary>
/// <remarks>
/// This attribute is intended for injecting dependencies into properties or fields of a class.
/// Below are some important considerations and limitations associated with using this attribute:
/// 1. **Read-Only Fields**: Fields marked as read-only cannot have dependencies injected into them
///    via this attribute. Use constructor injection for read-only fields.
/// 2. **Initialization**: Dependencies are injected after the instance of the object has been created.
///    Therefore, dependencies injected using this attribute are not available in the constructor of the class.
/// 3. **Property Setters**: For properties, a public or internal setter is necessary as the attribute
///    relies on it to inject the dependency. Read-only properties will not have dependencies injected.
/// 4. **Explicit Naming and Tagging**: The <c>Name</c> and <c>Tag</c> properties can be used to
///    resolve specific services if multiple services of the same type are registered.
/// 5. **Use Case Limitation**: This attribute is most suitable for classes where constructor injection
///    is not feasible or the class has a large number of dependencies, making constructor injection impractical.
///    However, constructor injection is generally recommended when possible for better clarity and to avoid
///    unintentionally overriding already set dependencies.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name associated with the service to resolve.
    /// </summary>
    /// <remarks>
    /// Use this property to specify the name of the service when resolving.
    /// It is particularly useful when multiple services of the same type are registered
    /// and a specific one needs to be resolved.
    /// </remarks>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the tag associated with the service to resolve.
    /// </summary>
    /// <remarks>
    /// Use this property to specify the tag of the service when resolving.
    /// It is particularly useful when multiple services of the same type are registered
    /// with different tags and a specific one needs to be resolved.
    /// </remarks>
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
