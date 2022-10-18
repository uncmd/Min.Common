using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Min.DependencyInjection;

public class DefaultTypeProvider : TypeProviderBase
{
    public override List<MinServiceDescriptor> GetServiceDescriptors(List<Type> types)
        => GetServiceDescriptorCore(types, typeof(ISingletonDependency), ServiceLifetime.Singleton)
            .Concat(GetServiceDescriptorCore(types, typeof(IScopedDependency), ServiceLifetime.Scoped))
            .Concat(GetServiceDescriptorCore(types, typeof(ITransientDependency), ServiceLifetime.Transient)).ToList();

    public virtual List<MinServiceDescriptor> GetServiceDescriptorCore(List<Type> types, Type type, ServiceLifetime lifetime)
    {
        List<MinServiceDescriptor> descriptors = new();
        var serviceTypes = GetServiceTypes(types, type);
        foreach (var serviceType in serviceTypes)
        {
            var implementationTypes = GetImplementationTypes(types, serviceType);
            foreach (var implementationType in implementationTypes)
            {
                if (serviceType.IsGenericType &&
                    implementationType.IsGenericType &&
                    serviceType.GetTypeInfo().GenericTypeParameters.Length != implementationType.GetTypeInfo().GenericTypeParameters.Length)
                    continue;

                descriptors.Add(new MinServiceDescriptor(serviceType, implementationType, lifetime));
            }
        }

        return descriptors;
    }

    public virtual List<Type> GetImplementationTypes(List<Type> types, Type serviceType)
    {
        if (serviceType.IsInterface)
            return types.Where(t => !t.IsAbstract && t.IsClass && IsAssignableFrom(serviceType, t)).ToList();

        return new List<Type>
        {
            serviceType
        };
    }

    public virtual List<Type> GetServiceTypes(List<Type> types, Type interfaceType)
    {
        var interfaceServiceTypes = types.Where(t => t.IsInterface && t != interfaceType && interfaceType.IsAssignableFrom(t));
        return types.Where(type
                => IsAssignableFrom(interfaceType, type) && !type.GetInterfaces().Any(t => interfaceServiceTypes.Contains(t)) &&
                !IsSkip(type))
            .Concat(interfaceServiceTypes)
            .ToList();
    }

    public virtual bool IsSkip(Type type)
    {
        if (type.IsAbstract)
            return true;

        var ignoreInjection = type.GetCustomAttribute<IgnoreInjectionAttribute>();
        if (ignoreInjection == null)
            return false;

        var inheritIgnoreInjection = type.GetCustomAttribute<IgnoreInjectionAttribute>(false);
        if (inheritIgnoreInjection != null)
            return true;

        return ignoreInjection.Inherit;
    }
}
