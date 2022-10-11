using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class MinCollectionExtensions
{
    public static bool IsNullOrEmpty<T>([MaybeNull] this ICollection<T> source)
    {
        return source == null || source.Count <= 0;
    }
}
