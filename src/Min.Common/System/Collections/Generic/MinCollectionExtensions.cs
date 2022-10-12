﻿using Min.Common;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class MinCollectionExtensions
{
    public static bool IsNullOrEmpty<T>([MaybeNull] this ICollection<T> source)
    {
        return source == null || source.Count <= 0;
    }

    public static bool AddIfNotContains<T>([NotNull] this ICollection<T> source, T item)
    {
        Check.NotNull(source);

        if (source.Contains(item))
        {
            return false;
        }

        source.Add(item);
        return true;
    }

    public static IEnumerable<T> AddIfNotContains<T>([NotNull] this ICollection<T> source, IEnumerable<T> items)
    {
        Check.NotNull(source);

        var addedItems = new List<T>();

        foreach (var item in items)
        {
            if (source.Contains(item))
            {
                continue;
            }

            source.Add(item);
            addedItems.Add(item);
        }

        return addedItems;
    }

    public static bool AddIfNotContains<T>([NotNull] this ICollection<T> source, [NotNull] Func<T, bool> predicate, [NotNull] Func<T> itemFactory)
    {
        Check.NotNull(source);
        Check.NotNull(predicate);
        Check.NotNull(itemFactory);

        if (source.Any(predicate))
        {
            return false;
        }

        source.Add(itemFactory());
        return true;
    }

    public static IList<T> RemoveAll<T>([NotNull] this ICollection<T> source, Func<T, bool> predicate)
    {
        var items = source.Where(predicate).ToList();

        foreach (var item in items)
        {
            source.Remove(item);
        }

        return items;
    }

    public static void RemoveAll<T>([NotNull] this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Remove(item);
        }
    }
}
