namespace FlexInject.Abstractions;

public interface IResolvePolicy
{
    object Resolve(FlexInjectContainer container, Type type, string name, string tag);
}