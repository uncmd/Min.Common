using Min.Common;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading.Tasks;

public static class MinTaskFactoryExtensions
{
    public static Task Run([NotNull] this TaskFactory taskFactory, [NotNull] Action action)
    {
        Check.NotNull(taskFactory);
        Check.NotNull(action);

        return taskFactory.StartNew(action, taskFactory.CancellationToken, taskFactory.CreationOptions | TaskCreationOptions.DenyChildAttach, taskFactory.Scheduler ?? TaskScheduler.Default);
    }

    public static Task<TResult> Run<TResult>([NotNull] this TaskFactory taskFactory, [NotNull] Func<TResult> action)
    {
        Check.NotNull(taskFactory);
        Check.NotNull(action);

        return taskFactory.StartNew(action, taskFactory.CancellationToken, taskFactory.CreationOptions | TaskCreationOptions.DenyChildAttach, taskFactory.Scheduler ?? TaskScheduler.Default);
    }

    public static Task Run([NotNull] this TaskFactory taskFactory, [NotNull] Func<Task> action)
    {
        Check.NotNull(taskFactory);
        Check.NotNull(action);

        return taskFactory.StartNew(action, taskFactory.CancellationToken, taskFactory.CreationOptions | TaskCreationOptions.DenyChildAttach, taskFactory.Scheduler ?? TaskScheduler.Default).Unwrap();
    }

    public static Task<TResult> Run<TResult>([NotNull] this TaskFactory taskFactory, [NotNull] Func<Task<TResult>> action)
    {
        Check.NotNull(taskFactory);
        Check.NotNull(action);

        return taskFactory.StartNew(action, taskFactory.CancellationToken, taskFactory.CreationOptions | TaskCreationOptions.DenyChildAttach, taskFactory.Scheduler ?? TaskScheduler.Default).Unwrap();
    }
}
