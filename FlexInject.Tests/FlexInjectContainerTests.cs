using FlexInject;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlexInjectTests;

public class FlexInjectContainerTests
{
    [Fact]
    public void Register_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        Assert.IsType<Sample>(container.Resolve<ISample>());
    }

    [Fact]
    public void Register_AlreadyRegisteredType_ShouldThrowException()
    {
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => container.Register<ISample, Sample>());
    }

    [Fact]
    public void RegisterTransient_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer();
        container.RegisterTransient<ISample, Sample>();
        Assert.IsType<Sample>(container.Resolve<ISample>());
    }

    [Fact]
    public void RegisterTransient_AlreadyRegisteredType_ShouldThrowException()
    {
        var container = CreateContainer();
        container.RegisterTransient<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => container.RegisterTransient<ISample, Sample>());
    }

    [Fact]
    public void RegisterScoped_ValidTypes_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer();
        container.RegisterScoped<ISample, Sample>();

        using (container.CreateScope())
        {
            Assert.IsType<Sample>(container.Resolve<ISample>());
        }
    }

    [Fact]
    public void RegisterScoped_AlreadyRegisteredType_ShouldThrowException()
    {
        var container = CreateContainer();
        container.RegisterScoped<ISample, Sample>();
        Assert.Throws<InvalidOperationException>(() => container.RegisterScoped<ISample, Sample>());
    }

    [Fact]
    public void RegisterSingleton_ValidInstance_ShouldRegisterSuccessfully()
    {
        var container = CreateContainer();
        var instance = new Sample();
        container.RegisterSingleton<ISample>(instance);
        Assert.Equal(instance, container.Resolve<ISample>());
    }

    [Fact]
    public void RegisterSingleton_NullInstance_ShouldThrowException()
    {
        var container = CreateContainer();
        Assert.Throws<ArgumentNullException>(() => container.RegisterSingleton<ISample>(null));
    }

    [Fact]
    public void RegisterSingleton_AlreadyRegisteredInstance_ShouldThrowException()
    {
        var container = CreateContainer();
        var instance = new Sample();
        container.RegisterSingleton<ISample>(instance);
        Assert.Throws<InvalidOperationException>(() => container.RegisterSingleton<ISample>(new Sample()));
    }

    [Fact]
    public void Resolve_UnregisteredType_ShouldThrowException()
    {
        var container = CreateContainer();
        Assert.Throws<InvalidOperationException>(() => container.Resolve<ISample>());
    }

    [Fact]
    public void Resolve_RegisteredType_ShouldReturnInstance()
    {
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        Assert.IsType<Sample>(container.Resolve<ISample>());
    }

    [Fact]
    public void Resolve_CyclicDependency_ShouldThrowException()
    {
        var container = CreateContainer();
        container.Register<ISample, CyclicSample>();
        Assert.Throws<InvalidOperationException>(() => container.Resolve<ISample>());
    }

    [Fact]
    public void CreateInstance_RegisteredType_ShouldCreateInstance()
    {
        var container = CreateContainer();
        container.Register<ISample, Sample>();
        var instance = container.Resolve<ISample>();
        Assert.IsType<Sample>(instance);
    }

    [Fact]
    public void CreateInstance_RegisteredTypeWithInitialize_ShouldCallInitialize()
    {
        var container = CreateContainer();
        container.Register<IInitializableSample, InitializableSample>();
        var instance = (InitializableSample)container.Resolve<IInitializableSample>();
        Assert.True(instance.Initialized);
    }

    [Fact]
    public void Dispose_DisposableInstance_ShouldDispose()
    {
        var container = CreateContainer();
        var instance = new DisposableSample();
        container.RegisterSingleton<IDisposableSample>(instance);
        container.Dispose();
        Assert.True(instance.Disposed);
    }

    private FlexInjectContainer CreateContainer()
    {
        var mockLogger = new Mock<ILogger<FlexInjectContainer>>();
        return new FlexInjectContainer(mockLogger.Object);
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

    public void Dispose() => Disposed = true;
}

public class CyclicSample : ISample
{
    public CyclicSample(ISample sample) { }
}
