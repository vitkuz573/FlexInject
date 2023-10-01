using FlexInject;
using FlexInject.Abstractions;
using FlexInject.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace FlexInjectTests;

public class FlexInjectContainerTests
{
    [Fact]
    public void Register_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer(services => services.AddTransient<ISample, Sample>());
        Assert.IsType<Sample>(container.GetService<ISample>());
    }

    [Fact]
    public void RegisterTransient_AlreadyRegisteredType_ShouldThrowException()
    {
        var services = new FlexServiceCollection();
        services.AddTransient<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => services.AddTransient<ISample, Sample>());
    }

    [Fact]
    public void RegisterTransient_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer(services => services.AddTransient<ISample, Sample>());
        Assert.IsType<Sample>(container.GetService<ISample>());
    }

    [Fact]
    public void RegisterScoped_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer(services => services.AddScoped<ISample, Sample>());

        using (container.CreateScope())
        {
            Assert.IsType<Sample>(container.GetService<ISample>());
        }
    }

    [Fact]
    public void RegisterScoped_AlreadyRegisteredType_ShouldThrowException()
    {
        var services = new FlexServiceCollection();
        services.AddScoped<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => services.AddScoped<ISample, Sample>());
    }

    [Fact]
    public void RegisterSingleton_ValidType_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer(services => services.AddSingleton<ISample, Sample>());
        Assert.IsType<Sample>(container.GetService<ISample>());
    }

    [Fact]
    public void RegisterSingleton_AlreadyRegisteredType_ShouldThrowException()
    {
        var services = new FlexServiceCollection();
        services.AddSingleton<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => services.AddSingleton<ISample, Sample>());
    }

    [Fact]
    public void Resolve_UnregisteredType_ShouldThrowException()
    {
        var container = CreateContainer();
        Assert.Throws<InvalidOperationException>(() => container.GetService<ISample>());
    }

    [Fact]
    public void Resolve_RegisteredType_ShouldReturnInstance()
    {
        var container = CreateContainer(services => services.AddTransient<ISample, Sample>());
        Assert.IsType<Sample>(container.GetService<ISample>());
    }

    [Fact]
    public void Resolve_CyclicDependency_ShouldThrowException()
    {
        var container = CreateContainer(services => services.AddTransient<ISample, CyclicSample>());
        Assert.Throws<InvalidOperationException>(() => container.GetService<ISample>());
    }

    [Fact]
    public void CreateInstance_RegisteredTypeWithInitialize_ShouldCallInitialize()
    {
        var container = CreateContainer(services => services.AddTransient<IInitializableSample, InitializableSample>());
        var instance = (InitializableSample)container.GetService<IInitializableSample>();
        Assert.True(instance.Initialized);
    }

    [Fact]
    public void Dispose_DisposableInstance_ShouldDispose()
    {
        var container = CreateContainer(services => services.AddSingleton<IDisposableSample, DisposableSample>());
        var resolvedInstance = container.GetService<IDisposableSample>() as DisposableSample;

        container.Dispose();

        Assert.NotNull(resolvedInstance);
        Assert.True(resolvedInstance.Disposed);
    }

    [Fact]
    public void Resolve_InjectAttributeOnField_ShouldInjectSuccessfully()
    {
        var container = CreateContainer(services =>
        {
            services.AddTransient<ISample, Sample>();
            services.AddTransient<ClassWithInjectedField, ClassWithInjectedField>();
        });

        var instance = container.GetService<ClassWithInjectedField>();
        Assert.NotNull(instance.Sample);
        Assert.IsType<Sample>(instance.Sample);
    }

    [Fact]
    public void Resolve_InjectAttributeOnProperty_ShouldInjectSuccessfully()
    {
        var container = CreateContainer(services =>
        {
            services.AddTransient<ISample, Sample>();
            services.AddTransient<ClassWithInjectedProperty, ClassWithInjectedProperty>();
        });

        var instance = container.GetService<ClassWithInjectedProperty>();
        Assert.NotNull(instance.Sample);
        Assert.IsType<Sample>(instance.Sample);
    }

    [Fact]
    public void Resolve_UsingPolicy_ShouldResolveSuccessfully()
    {
        var container = CreateContainer(services => services.AddPolicy(new SampleResolvePolicy()));
        var instance = container.GetService<ISample>();
        Assert.NotNull(instance);
        Assert.IsType<Sample>(instance);
    }

    [Fact]
    public void CreateScope_ScopedInstance_ShouldBeDifferentInDifferentScopes()
    {
        var container = CreateContainer(services => services.AddScoped<ISample, Sample>());
        ISample instance1, instance2;

        using (container.CreateScope())
        {
            instance1 = container.GetService<ISample>();
        }

        using (container.CreateScope())
        {
            instance2 = container.GetService<ISample>();
        }

        Assert.NotEqual(instance1, instance2);
    }

    [Fact]
    public void Resolve_TypeWithConstructorParameters_ShouldResolveSuccessfully()
    {
        var container = CreateContainer(services =>
        {
            services.AddTransient<IService, ServiceImplementation>();
            services.AddTransient<IConsumer, Consumer>();
        });

        var consumer = container.GetService<IConsumer>();

        Assert.NotNull(consumer);
        Assert.NotNull(consumer.Service);
        Assert.IsType<ServiceImplementation>(consumer.Service);
    }

    [Fact]
    public void CreateScope_ScopedInstance_ShouldBeSameInSameScope()
    {
        var container = CreateContainer(services => services.AddScoped<ISample, Sample>());
        ISample instance1, instance2;

        using (var scope = container.CreateScope())
        {
            instance1 = scope.ServiceProvider.GetService<ISample>();
            instance2 = scope.ServiceProvider.GetService<ISample>();
        }

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void CreateScope_NestedScopes_ShouldCreateDifferentInstances()
    {
        var container = CreateContainer(services => services.AddScoped<ISample, Sample>());
        ISample parentInstance, childInstance;

        using (var parentScope = container.CreateScope())
        {
            parentInstance = parentScope.ServiceProvider.GetService<ISample>();

            using (var childScope = parentScope.ServiceProvider.CreateScope())
            {
                childInstance = childScope.ServiceProvider.GetService<ISample>();
            }
        }

        Assert.NotSame(parentInstance, childInstance);
    }

    [Fact]
    public void CreateScope_Singleton_ShouldBeSameInDifferentScopes()
    {
        var container = CreateContainer(services => services.AddSingleton<ISample, Sample>());
        ISample instance1, instance2;

        using (container.CreateScope())
        {
            instance1 = container.GetService<ISample>();
        }

        using (container.CreateScope())
        {
            instance2 = container.GetService<ISample>();
        }

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Resolve_TransitiveDependency_ShouldResolveSuccessfully()
    {
        var container = CreateContainer(services =>
        {
            services.AddTransient<IConsumer, Consumer>();
            services.AddTransient<IService, ServiceImplementation>();
        });

        var consumer = container.GetService<IConsumer>();

        Assert.NotNull(consumer);
        Assert.IsType<Consumer>(consumer);
        Assert.NotNull(consumer.Service);
        Assert.IsType<ServiceImplementation>(consumer.Service);
    }

    [Fact]
    public void Resolve_WithDifferentNames_ShouldResolveDifferentInstances()
    {
        var container = CreateContainer(services =>
        {
            services.AddSingleton<ISample, Sample>("name1");
            services.AddSingleton<ISample, Sample>("name2");
        });

        var instance1 = container.GetService<ISample>("name1");
        var instance2 = container.GetService<ISample>("name2");

        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.NotSame(instance1, instance2);
    }

    [Fact]
    public void Register_WithSameNameAndTag_ShouldThrowException()
    {
        var services = new FlexServiceCollection();
        services.AddSingleton<ISample, Sample>("name", "tag");
        Assert.Throws<InvalidOperationException>(() => services.AddSingleton<ISample, Sample>("name", "tag"));
    }

    private static IFlexServiceProvider CreateContainer(Action<FlexServiceCollection> configureServices = null)
    {
        var services = new FlexServiceCollection();
        configureServices?.Invoke(services);

        return services.BuildServiceProvider();
    }
}

public interface ISample { }

public class Sample : ISample { }

public interface IInitializableSample : IInitialize { }

public class InitializableSample : IInitializableSample
{
    public bool Initialized { get; private set; }

    public void Initialize() => Initialized = true;
}

public interface IDisposableSample : IDisposable
{
    bool Disposed { get; }
}

public class DisposableSample : IDisposableSample
{
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class CyclicSample : ISample
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Intentionally left for testing purposes.")]
    public CyclicSample(ISample sample) { }
}

public class ClassWithInjectedField
{
    [Inject]
    public ISample Sample;
}

public class ClassWithInjectedProperty
{
    [Inject]
    public ISample Sample { get; set; }
}

public class SampleResolvePolicy : IResolvePolicy
{
    public object Resolve(FlexInjectContainer container, Type type, string name, string tag)
    {
        return type == typeof(ISample) ? new Sample() : (object?)null;
    }
}

public interface IService { }
public class ServiceImplementation : IService { }

public interface IConsumer
{
    IService Service { get; }
}

public class Consumer : IConsumer
{
    public IService Service { get; }

    public Consumer(IService service)
    {
        Service = service;
    }
}