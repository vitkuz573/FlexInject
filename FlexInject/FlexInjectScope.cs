using FlexInject.Models;
using System.Collections.Concurrent;

namespace FlexInject;

/// <summary>
/// Represents a scope within which scoped dependencies are cached.
/// When the scope is disposed, all cached instances are disposed as well.
/// </summary>
internal class FlexInjectScope : IDisposable
{
    private readonly ConcurrentDictionary<InjectionKey, object> _scopedInstances = new();

    public object GetOrAdd(InjectionKey key, Func<object> factory)
    {
        return _scopedInstances.GetOrAdd(key, _ => factory());
    }

    public IReadOnlyCollection<object> Instances => (IReadOnlyCollection<object>)_scopedInstances.Values;

    public void Dispose()
    {
        foreach (var instance in _scopedInstances.Values)
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _scopedInstances.Clear();
    }
}
