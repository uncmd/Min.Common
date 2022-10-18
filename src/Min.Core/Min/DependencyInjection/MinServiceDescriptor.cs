using Microsoft.Extensions.DependencyInjection;

namespace Min.DependencyInjection;

public class MinServiceDescriptor
{
    public Type ServiceType { get; }

    public Type ImplementationType { get; }

    public ServiceLifetime Lifetime { get; }

    public MinServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }
}
