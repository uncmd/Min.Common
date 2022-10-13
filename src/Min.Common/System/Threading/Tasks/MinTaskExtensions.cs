using Min.Common;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading.Tasks;

public static class MinTaskExtensions
{
    public static void WaitAndUnwrapException([NotNull] this Task task)
    {
        Check.NotNull(task);

        task.GetAwaiter().GetResult();
    }

    public static void WaitAndUnwrapException([NotNull] this Task task, CancellationToken cancellationToken)
    {
        Check.NotNull(task);

        try
        {
            task.Wait(cancellationToken);
        }
        catch (AggregateException ex)
        {
            if (ex.InnerException != null)
            {
                throw ex.InnerException.ReThrow();
            }
            else
            {
                throw;
            }
        }
    }

    public static TResult WaitAndUnwrapException<TResult>([NotNull] this Task<TResult> task)
    {
        Check.NotNull(task);

        return task.GetAwaiter().GetResult();
    }

    public static TResult WaitAndUnwrapException<TResult>([NotNull] this Task<TResult> task, CancellationToken cancellationToken)
    {
        Check.NotNull(task);

        try
        {
            task.Wait(cancellationToken);
            return task.Result;
        }
        catch (AggregateException ex)
        {
            if (ex.InnerException != null)
            {
                throw ex.InnerException.ReThrow();
            }
            else
            {
                throw;
            }
        }
    }

    public static void WaitWithoutException([NotNull] this Task task)
    {
        Check.NotNull(task);

        try
        {
            task.Wait();
        }
        catch (AggregateException)
        {
        }
    }

    public static void WaitWithoutException([NotNull] this Task task, CancellationToken cancellationToken)
    {
        Check.NotNull(task);

        try
        {
            task.Wait(cancellationToken);
        }
        catch (AggregateException)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
