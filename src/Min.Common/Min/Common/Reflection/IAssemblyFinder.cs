using System.Reflection;

namespace Min.Common.Reflection;

public interface IAssemblyFinder
{
    IReadOnlyList<Assembly> Assemblies { get; }
}
