using FlexInject.Abstractions;
using FlexInject.Attributes;

namespace FlexInject.Tests;

public class FlexInjectContainerTests
{
    [Fact]
    public void Register_ValidTypes_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = CreateContainer();

        // Act
        container.Register<ISample, Sample>();
        var resolvedInstance = container.Resolve<ISample>();

        // Assert
        Assert.IsType<Sample>(resolvedInstance);
    }

    [Fact]
    public void Register_AlreadyRegisteredType_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.Register<ISample, Sample>());
    }

    [Fact]
    public void RegisterTransient_ValidTypes_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = CreateContainer();

        // Act
        container.RegisterTransient<ISample, Sample>();
        var resolvedInstance = container.Resolve<ISample>();

        // Assert
        Assert.IsType<Sample>(resolvedInstance);
    }

    [Fact]
    public void RegisterTransient_AlreadyRegisteredType_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterTransient<ISample, Sample>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.RegisterTransient<ISample, Sample>());
    }

    [Fact]
    public void RegisterScoped_ValidTypes_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterScoped<ISample, Sample>();

        // Act
        using (container.CreateScope())
        {
            var resolvedInstance = container.Resolve<ISample>();

            // Assert
            Assert.IsType<Sample>(resolvedInstance);
        }
    }

    [Fact]
    public void RegisterScoped_AlreadyRegisteredType_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterScoped<ISample, Sample>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.RegisterScoped<ISample, Sample>());
    }

    [Fact]
    public void RegisterSingleton_ValidType_ShouldRegisterSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterSingleton<ISample, Sample>();

        // Act
        var resolvedInstance = container.Resolve<ISample>();

        // Assert
        Assert.IsType<Sample>(resolvedInstance);
    }

    [Fact]
    public void RegisterSingleton_AlreadyRegisteredType_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterSingleton<ISample, Sample>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.RegisterSingleton<ISample, Sample>());
    }

    [Fact]
    public void Resolve_UnregisteredType_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.Resolve<ISample>());
    }

    [Fact]
    public void Resolve_RegisteredType_ShouldReturnInstance()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>();

        // Act
        var resolvedInstance = container.Resolve<ISample>();

        // Assert
        Assert.IsType<Sample>(resolvedInstance);
    }

    [Fact]
    public void Resolve_CyclicDependency_ShouldThrowException()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, CyclicSample>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => container.Resolve<ISample>());
    }

    [Fact]
    public void CreateInstance_RegisteredType_ShouldCreateInstance()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>();

        // Act
        var instance = container.Resolve<ISample>();

        // Assert
        Assert.IsType<Sample>(instance);
    }

    [Fact]
    public void CreateInstance_RegisteredTypeWithInitialize_ShouldCallInitialize()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<IInitializableSample, InitializableSample>();

        // Act
        var instance = (InitializableSample)container.Resolve<IInitializableSample>();

        // Assert
        Assert.True(instance.Initialized);
    }

    [Fact]
    public void Dispose_DisposableInstance_ShouldDispose()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterSingleton<IDisposableSample, DisposableSample>();

        // Act
        var resolvedInstance = container.Resolve<IDisposableSample>() as DisposableSample;
        container.Dispose();

        // Assert
        Assert.NotNull(resolvedInstance);
        Assert.True(resolvedInstance.Disposed);
    }

    [Fact]
    public void Resolve_InjectAttributeOnField_ShouldInjectSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        container.Register<ClassWithInjectedField, ClassWithInjectedField>();

        // Act
        var instance = container.Resolve<ClassWithInjectedField>();

        // Assert
        Assert.NotNull(instance.Sample);
        Assert.IsType<Sample>(instance.Sample);
    }

    [Fact]
    public void Resolve_InjectAttributeOnProperty_ShouldInjectSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        container.Register<ClassWithInjectedProperty, ClassWithInjectedProperty>();

        // Act
        var instance = container.Resolve<ClassWithInjectedProperty>();

        // Assert
        Assert.NotNull(instance.Sample);
        Assert.IsType<Sample>(instance.Sample);
    }

    [Fact]
    public void Resolve_WithDifferentNameAndTag_ShouldResolveSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<ISample, Sample>("name", "tag");

        // Act
        var instance = container.Resolve<ISample>("name", "tag");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<Sample>(instance);
    }

    [Fact]
    public void Resolve_UsingPolicy_ShouldResolveSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.AddPolicy(new SampleResolvePolicy());

        // Act
        var instance = container.Resolve<ISample>();

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<Sample>(instance);
    }

    [Fact]
    public void CreateScope_ScopedInstance_ShouldBeDifferentInDifferentScopes()
    {
        // Arrange
        var container = CreateContainer();
        container.RegisterScoped<ISample, Sample>();
        ISample instance1, instance2;

        // Act
        using (container.CreateScope())
        {
            instance1 = container.Resolve<ISample>();
        }

        using (container.CreateScope())
        {
            instance2 = container.Resolve<ISample>();
        }

        // Assert
        Assert.NotEqual(instance1, instance2);
    }

    [Fact]
    public void Resolve_TypeWithConstructorParameters_ShouldResolveSuccessfully()
    {
        // Arrange
        var container = CreateContainer();
        container.Register<IService, ServiceImplementation>();
        container.Register<IConsumer, Consumer>();

        // Act
        var consumer = container.Resolve<IConsumer>();

        // Assert
        Assert.NotNull(consumer);
        Assert.NotNull(consumer.Service);
        Assert.IsType<ServiceImplementation>(consumer.Service);
    }

    private static FlexInjectContainer CreateContainer()
    {
        return new FlexInjectContainer();
    }
}

#region Supporting Test Types

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

public class CyclicSample(ISample sample) : ISample
{
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

public class Consumer(IService service) : IConsumer
{
    public IService Service { get; } = service;
}

#endregion
