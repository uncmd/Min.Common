using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Min.Reflection;

public static class AssemblyHelper
{
    public static List<Assembly> LoadAssemblies(string folderPath, SearchOption searchOption)
    {
        return GetAssemblyFiles(folderPath, searchOption)
            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
            .ToList();
    }

    public static IEnumerable<string> GetAssemblyFiles(string folderPath, SearchOption searchOption)
    {
        return Directory
            .EnumerateFiles(folderPath, "*.*", searchOption)
            .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe"));
    }

    public static IReadOnlyList<Type> GetAllTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
    }

    public static List<Assembly> FindAllAssemblies(ILogger logger)
    {
        return FindAllAssemblies(Assembly.GetEntryAssembly(), logger);
    }

    public static List<Assembly> FindAllAssemblies(Assembly startupAssembly, ILogger logger)
    {
        var projectAssemblies = new List<Assembly>();
        logger.Log(LogLevel.Information, "Loaded project assemblies:");
        AddAssemblyAndDependenciesRecursively(projectAssemblies, startupAssembly, logger);
        return projectAssemblies;
    }

    private static void AddAssemblyAndDependenciesRecursively(
        List<Assembly> projectAssemblies,
        Assembly assembly,
        ILogger logger,
        int depth = 0)
    {
        if (projectAssemblies.Contains(assembly))
        {
            return;
        }

        projectAssemblies.Add(assembly);
        logger.Log(LogLevel.Information, $"{new string(' ', depth * 2)}- {assembly.GetName().Name}");

        foreach (var dependedAssembly in FindDependedAssemblies(assembly))
        {
            AddAssemblyAndDependenciesRecursively(projectAssemblies, dependedAssembly, logger, depth + 1);
        }
    }

    public static List<Assembly> FindDependedAssemblies(Assembly assembly)
    {
        return DependencyContext.Default.CompileLibraries
            .Where(p => p.Name == assembly.GetName().Name)
            .FirstOrDefault()
            ?.Dependencies
            .Select(p => AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(p.Name)))
            .ToList();
    }

    public static List<Assembly> FindAllDependencyAssemblies(ILogger logger)
    {
        return FindAllDependencyAssemblies(Assembly.GetEntryAssembly(), logger);
    }

    public static List<Assembly> FindAllDependencyAssemblies(Assembly startupAssembly, ILogger logger)
    {
        var assemblies = new List<AssemblyDescriptor>();

        FillAssemblies(assemblies, startupAssembly, logger);
        SetDependencies(assemblies);
        assemblies = SortByDependency(assemblies, startupAssembly);

        return assemblies.Select(p => p.Assembly).ToList();
    }

    private static void FillAssemblies(List<AssemblyDescriptor> assemblies, Assembly startupAssembly, ILogger logger)
    {
        foreach (var assembly in FindAllAssemblies(startupAssembly, logger))
        {
            assemblies.Add(new AssemblyDescriptor(assembly));
        }
    }

    private static void SetDependencies(List<AssemblyDescriptor> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            SetDependencies(assemblies, assembly);
        }
    }

    private static void SetDependencies(List<AssemblyDescriptor> assemblies, AssemblyDescriptor assembly)
    {
        foreach (var dependedAssemblyType in FindDependedAssemblies(assembly.Assembly))
        {
            var dependedAssembly = assemblies.FirstOrDefault(m => m.Assembly == dependedAssemblyType);
            if (dependedAssembly == null)
            {
                throw new MinCommonException("Could not find a depended assembly " + dependedAssemblyType.FullName + " for " + assembly.Assembly.FullName);
            }

            assembly.AddDependency(dependedAssembly);
        }
    }

    private static List<AssemblyDescriptor> SortByDependency(List<AssemblyDescriptor> assemblies, Assembly startupAssembly)
    {
        var sortedAssemblies = assemblies.SortByDependencies(m => m.Dependencies);
        sortedAssemblies.MoveItem(m => m.Assembly == startupAssembly, assemblies.Count - 1);
        return sortedAssemblies;
    }

    class AssemblyDescriptor
    {
        public Assembly Assembly { get; }

        public IReadOnlyList<AssemblyDescriptor> Dependencies => _dependencies.ToImmutableList();
        private readonly List<AssemblyDescriptor> _dependencies;

        public AssemblyDescriptor([NotNull] Assembly assembly)
        {
            Check.NotNull(assembly);

            Assembly = assembly;
            _dependencies = new List<AssemblyDescriptor>();
        }

        public void AddDependency(AssemblyDescriptor descriptor)
        {
            _dependencies.AddIfNotContains(descriptor);
        }

        public override string ToString()
        {
            return $"[AssemblyDescriptor {Assembly.FullName}]";
        }
    }
}
