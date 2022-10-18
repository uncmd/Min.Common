namespace Min.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute : Attribute
{
    /// <summary>
    /// Attempt to register only if not registered
    /// </summary>
    public virtual bool TryRegister { get; set; } = false;

    /// <summary>
    /// If the original service is already registered, replace the service registration, if not, register the service to DI
    /// </summary>
    public virtual bool ReplaceServices { get; set; }
}
