using System.Reflection;

namespace Min.Reflection;

public interface IAssemblyFinder
{
    IReadOnlyList<Assembly> Assemblies { get; }
}
