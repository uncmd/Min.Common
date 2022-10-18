using Microsoft.Extensions.DependencyInjection;

namespace Min.DependencyInjection;

public interface IServiceRegister
{
    void Add(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime);
}
