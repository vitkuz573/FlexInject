namespace FlexInject.Abstractions;

/// <summary>
/// Provides an interface for objects that require initialization. This interface can be used 
/// to perform any necessary setup or configuration after an object is constructed and all of its 
/// dependencies are resolved, but before it is used.
/// </summary>
public interface IInitialize
{
    /// <summary>
    /// Performs object initialization. This method is intended to be called once, after the object 
    /// is fully constructed and all dependencies are resolved.
    /// </summary>
    void Initialize();
}
