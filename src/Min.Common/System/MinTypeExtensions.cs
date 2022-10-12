using Min.Common;
using System.Diagnostics.CodeAnalysis;

namespace System;

public static class MinTypeExtensions
{
    public static bool IsAssignableTo<TTarget>([NotNull] this Type type)
    {
        Check.NotNull(type);

        return type.IsAssignableTo(typeof(TTarget));
    }

    public static bool IsAssignableTo([NotNull] this Type type, [NotNull] Type targetType)
    {
        Check.NotNull(type);
        Check.NotNull(targetType);

        return targetType.IsAssignableFrom(type);
    }

    public static string GetFullNameWithAssemblyName(this Type type)
    {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    public static Type[] GetBaseClasses([NotNull] this Type type, bool includeObject = true)
    {
        Check.NotNull(type);

        var types = new List<Type>();
        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject);
        return types.ToArray();
    }

    public static Type[] GetBaseClasses([NotNull] this Type type, Type stoppingType, bool includeObject = true)
    {
        Check.NotNull(type);

        var types = new List<Type>();
        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject, stoppingType);
        return types.ToArray();
    }

    private static void AddTypeAndBaseTypesRecursively(
        [NotNull] List<Type> types,
        [MaybeNull] Type? type,
        bool includeObject,
        [MaybeNull] Type? stoppingType = null)
    {
        if (type == null || type == stoppingType)
        {
            return;
        }

        if (!includeObject && type == typeof(object))
        {
            return;
        }

        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject, stoppingType);
        types.Add(type);
    }
}
