using System.ComponentModel;
using System.Globalization;

namespace System;

public static class MinObjectExtensions
{
    public static T As<T>(this object obj)
        where T : class
    {
        return (T)obj;
    }

    public static T To<T>(this object obj)
        where T : struct
    {
        if (typeof(T) == typeof(Guid))
        {
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.ToString());
        }

        return (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
    }

    public static bool IsIn<T>(this T item, params T[] list)
    {
        return list.Contains(item);
    }

    public static bool IsIn<T>(this T item, IEnumerable<T> items)
    {
        return items.Contains(item);
    }

    public static T If<T>(this T obj, bool condition, Action<T> action)
    {
        if (condition)
        {
            action(obj);
        }

        return obj;
    }

    public static T If<T>(this T obj, bool condition, Func<T, T> func)
    {
        if (condition)
        {
            return func(obj);
        }

        return obj;
    }
}
