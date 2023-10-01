namespace FlexInject.Abstractions;

public interface IFlexServiceScope : IDisposable
{
    IFlexServiceProvider ServiceProvider { get; }
}