using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Configuration;

public static class MinConfigurationExtensions
{
    public static IServiceCollection TryAddConfiguration(
        this IServiceCollection services,
        MinConfigurationBuilderOptions? options = null,
        Action<IConfigurationBuilder>? builderAction = null)
    {
        if (!services.IsAdded<IConfiguration>())
        {
            services.ReplaceConfiguration(ConfigurationHelper.BuildConfiguration(options, builderAction));
        }

        return services;
    }
}
