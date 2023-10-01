namespace FlexInject.Abstractions;

public interface IFlexServiceProvider : IDisposable
{
    object GetService(Type serviceType, string name = null, string tag = null);

    T GetService<T>(string name = null, string tag = null);

    IFlexServiceScope CreateScope();
}
