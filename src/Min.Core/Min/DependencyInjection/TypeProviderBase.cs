using System.Reflection;

namespace Min.DependencyInjection;

public abstract class TypeProviderBase : ITypeProvider
{
    public virtual bool IsAssignableFrom(Type type, Type targetType)
    {
        if (type.IsGenericType &&
            type.GetTypeInfo().GenericTypeParameters.Length > 0 &&
            targetType.IsGenericType &&
            targetType.GetTypeInfo().GenericTypeParameters.Length > 0)
            return targetType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == type);

        return type.IsAssignableFrom(targetType);
    }

    public virtual bool IsAssignableTo(Type type, Type targetType)
        => IsAssignableFrom(targetType, type);

    public virtual List<Type> GetAllTypes() => GetAllTypes(AppDomain.CurrentDomain.GetAssemblies());

    public virtual List<Type> GetAllTypes(IEnumerable<Assembly> assemblies)
        => assemblies.SelectMany(assembly => assembly.GetTypes()).ToList();

    public abstract List<MinServiceDescriptor> GetServiceDescriptors(List<Type> types);
}
