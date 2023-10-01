
## FlexInject: Lightweight DI Container for .NET

### Overview:
FlexInject is a lightweight, efficient, and flexible Dependency Injection container for .NET applications. It enables developers to manage the lifecycle of their objects, register dependencies, and resolve them at runtime while providing scope management.

### Features:
- **Type Registration**: Register types with optional names and tags.
- **Lifecycle Management**: Support for Transient, Scoped, and Singleton lifecycles.
- **Attribute Injection**: Inject dependencies using the `InjectAttribute` on fields and properties.
- **Custom Resolve Policies**: Extend resolving capabilities using custom policies.
- **Scope Management**: Create and manage scopes for resolving scoped instances.
- **Cyclic Dependency Detection**: Detects cyclic dependencies and throws informative exceptions.
- **Initialization Interface**: Optionally initialize resolved objects that implement the `IInitialize` interface.

### Quick Start:

#### 1. **Create a FlexInject Container:**
```csharp
var container = new FlexInjectContainer();
```

#### 2. **Register Types:**
```csharp
container.Register<IService, ServiceImplementation>();
container.RegisterSingleton<ISingletonService, SingletonServiceImplementation>();
container.RegisterTransient<ITransientService, TransientServiceImplementation>();
container.RegisterScoped<IScopedService, ScopedServiceImplementation>();
```

#### 3. **Resolve Types:**
```csharp
var service = container.Resolve<IService>();
```

#### 4. **Create a Scope:**
```csharp
using (var scope = container.CreateScope())
{
    var scopedService = container.Resolve<IScopedService>();
}
```

### Attribute Injection:
FlexInject supports attribute injection using the `InjectAttribute`, which can be applied to properties and fields. You can optionally specify name and tag via attributes.

```csharp
public class MyClass
{
    [Inject(Name = "specialService")]
    private readonly IService _service;
    
    [Inject(Tag = "taggedService")]
    public IAnotherService AnotherService { get; set; }
}
```

### Adding Resolve Policies:
You can extend the resolving capabilities of the container by implementing and adding custom `IResolvePolicy`.

```csharp
public class MyResolvePolicy : IResolvePolicy
{
    public object Resolve(FlexInjectContainer container, Type type, string name, string tag)
    {
        // Custom resolve logic here.
    }
}

container.AddPolicy(new MyResolvePolicy());
```

### Initialization:
Objects that implement the `IInitialize` interface will have their `Initialize` method called upon creation.

```csharp
public class InitializableObject : IInitialize
{
    public void Initialize()
    {
        // Initialization logic here.
    }
}
```

### Disposal:
Dispose of the container to release resources of IDisposable objects.

```csharp
container.Dispose();
```

### Documentation:
For more detailed information and advanced usage, please refer to the full documentation (https://github.com/vitkuz573/FlexInject/wiki).

### Download:
FlexInject is open-source and available for download [here](https://github.com/vitkuz573/FlexInject).

### Licensing:
FlexInject is licensed under the MIT License - see the [LICENSE](https://github.com/vitkuz573/FlexInject/blob/main/LICENSE) file for details.

### Contributing:
We welcome contributions! Please see our contributing guidelines (https://github.com/vitkuz573/FlexInject/blob/main/CONTRIBUTING.md) for more details.

### Issues:
If you encounter any issues or have feature requests, please open an issue [here](https://github.com/vitkuz573/FlexInject/issues).

### Conclusion:
FlexInject provides a lightweight and flexible solution for managing dependencies in .NET applications, allowing developers to focus on writing clean and maintainable code.
