using FlexInject.Abstractions;

namespace FlexInject;

internal class FlexServiceScope : IFlexServiceScope
{
    private readonly FlexInjectContainer _container;

    public FlexServiceScope(FlexInjectContainer container)
    {
        _container = container;
        ServiceProvider = _container;

        _container.InitializeScopedInstances();
    }

    public IFlexServiceProvider ServiceProvider { get; }

    public void Dispose()
    {
        // Cleanup logic for the scope if needed
    }
}
