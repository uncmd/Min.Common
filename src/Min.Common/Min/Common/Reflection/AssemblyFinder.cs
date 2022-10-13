using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Reflection;

namespace Min.Common.Reflection;

public class AssemblyFinder : IAssemblyFinder
{
    private readonly Lazy<IReadOnlyList<Assembly>> _assemblies;
    private readonly ILogger<AssemblyFinder> _logger;

    public AssemblyFinder(ILogger<AssemblyFinder> logger)
    {
        _logger = logger;
        _assemblies = new Lazy<IReadOnlyList<Assembly>>(FindAll, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IReadOnlyList<Assembly> Assemblies => _assemblies.Value;

    public IReadOnlyList<Assembly> FindAll()
    {
        var assemblies = new List<Assembly>();

        foreach (var assembly in AssemblyHelper.FindAllAssemblies(_logger))
        {
            assemblies.Add(assembly);
        }

        return assemblies.Distinct().ToImmutableList();
    }
}
