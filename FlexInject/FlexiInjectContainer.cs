using System.Collections.Concurrent;

namespace FlexInject;

public class FlexInjectContainer
{
    private readonly ConcurrentDictionary<Type, Type> _typeMapping = new ConcurrentDictionary<Type, Type>();
    private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();

    public void Register<TInterface, TImplementation>()
    {
        _typeMapping[typeof(TInterface)] = typeof(TImplementation);
    }

    public void RegisterSingleton<TInterface>(TInterface instance)
    {
        _singletonInstances[typeof(TInterface)] = instance;
    }

    public TInterface Resolve<TInterface>()
    {
        return (TInterface)Resolve(typeof(TInterface));
    }

    public object Resolve(Type type)
    {
        if (_singletonInstances.TryGetValue(type, out var singletonInstance))
        {
            return singletonInstance;
        }

        if (!_typeMapping.TryGetValue(type, out var implementationType))
        {
            throw new InvalidOperationException($"Type {type.FullName} is not registered.");
        }

        var constructor = implementationType.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var parameterInstances = parameters.Select(p => Resolve(p.ParameterType)).ToArray();

        return Activator.CreateInstance(implementationType, parameterInstances);
    }
}
