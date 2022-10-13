using System.Diagnostics.CodeAnalysis;

namespace Min;

public sealed class NullDisposable : IDisposable
{
    public static NullDisposable Instance { get; } = new NullDisposable();

    private NullDisposable()
    {

    }

    public void Dispose()
    {

    }
}

public sealed class NullAsyncDisposable : IAsyncDisposable
{
    public static NullAsyncDisposable Instance { get; } = new NullAsyncDisposable();

    private NullAsyncDisposable()
    {

    }

    public ValueTask DisposeAsync()
    {
        return default;
    }
}

public sealed class DisposeAction : IDisposable
{
    public static readonly DisposeAction Empty = new(null);

    private Action? _disposeAction;

    public DisposeAction(Action? disposeAction)
    {
        _disposeAction = disposeAction;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _disposeAction, null)?.Invoke();
    }
}

public class AsyncDisposeFunc : IAsyncDisposable
{
    private readonly Func<Task> _func;

    public AsyncDisposeFunc([NotNull] Func<Task> func)
    {
        Check.NotNull(func);

        _func = func;
    }

    public async ValueTask DisposeAsync()
    {
        await _func();
    }
}
