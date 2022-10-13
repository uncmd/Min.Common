using System.Reflection;

namespace Microsoft.Extensions.Configuration;

public class MinConfigurationBuilderOptions
{
    public Assembly UserSecretsAssembly { get; set; }

    public string UserSecretsId { get; set; }

    public string FileName { get; set; } = "appsettings";

    public bool Optional { get; set; } = true;

    public bool ReloadOnChange { get; set; } = true;

    public string EnvironmentName { get; set; }

    public string BasePath { get; set; }

    public string EnvironmentVariablesPrefix { get; set; }

    public string[] CommandLineArgs { get; set; }
}
