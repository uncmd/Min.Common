using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Min.Common;

[DebuggerStepThrough]
public static class Check
{
    public static T NotNull<T>(
        T? value,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        return value;
    }

    public static T NotNull<T>(
        T value,
        string message,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName, message);
        }

        return value;
    }

    public static string NotNullOrEmpty(
        string value,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        if (value.IsNullOrEmpty())
        {
            throw new ArgumentException($"{parameterName} can not be null or empty!", parameterName);
        }

        return value;
    }

    public static string NotNullOrWhiteSpace(
        string value,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        if (value.IsNullOrWhiteSpace())
        {
            throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName);
        }

        return value;
    }

    public static ICollection<T> NotNullOrEmpty<T>(
        ICollection<T> value,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        if (value.IsNullOrEmpty())
        {
            throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
        }

        return value;
    }

    public static Type AssignableTo<TBaseType>(
        Type type,
        [CallerArgumentExpression("type")] string? parameterName = default)
    {
        NotNull(type, parameterName);

        if (!type.IsAssignableTo<TBaseType>())
        {
            throw new ArgumentException($"{parameterName} (type of {type.AssemblyQualifiedName}) should be assignable to the {typeof(TBaseType).GetFullNameWithAssemblyName()}!");
        }

        return type;
    }

    public static string Length(
        [MaybeNull] string value,
        int maxLength,
        int minLength = 0,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        if (minLength > 0)
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
            }

            if (value.Length < minLength)
            {
                throw new ArgumentException($"{parameterName} length must be equal to or bigger than {minLength}!", parameterName);
            }
        }

        if (value != null && value.Length > maxLength)
        {
            throw new ArgumentException($"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
        }

        return value;
    }

    public static T NotDefaultOrNull<T>(
        T? value,
        [CallerArgumentExpression("value")] string? parameterName = default)
        where T : struct
    {
        if (value == null)
        {
            throw new ArgumentException($"{parameterName} is null!", parameterName);
        }

        if (value.Value.Equals(default(T)))
        {
            throw new ArgumentException($"{parameterName} has a default value!", parameterName);
        }

        return value.Value;
    }

    public static T Ensure<T>(
        Func<T, bool> condition, 
        T value, 
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        NotNull(condition);
        if (!condition(value))
        {
            throw new ArgumentException($"{parameterName} does not meet condition", parameterName);
        }
        return value;
    }

    public static async Task<T> EnsureAsync<T>(
        Func<T, Task<bool>> condition, 
        T value, 
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        NotNull(condition);
        if (!await condition(value))
        {
            throw new ArgumentException($"{parameterName} does not meet condition", parameterName);
        }
        return value;
    }

    public static async Task<T> EnsureAsync<T>(
        Func<T, ValueTask<bool>> condition,
        T value,
        [CallerArgumentExpression("value")] string? parameterName = default)
    {
        NotNull(condition);
        if (!await condition(value))
        {
            throw new ArgumentException($"{parameterName} does not meet condition", parameterName);
        }
        return value;
    }
}
