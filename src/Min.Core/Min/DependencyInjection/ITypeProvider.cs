using System.Reflection;

namespace Min.DependencyInjection;

public interface ITypeProvider
{
    bool IsAssignableFrom(Type type, Type targetType);

    bool IsAssignableTo(Type type, Type targetType);

    List<Type> GetAllTypes();

    List<Type> GetAllTypes(IEnumerable<Assembly> assemblies);

    List<MinServiceDescriptor> GetServiceDescriptors(List<Type> types);
}
