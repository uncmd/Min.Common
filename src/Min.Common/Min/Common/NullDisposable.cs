namespace Min.Common;

public sealed class NullDisposable : IDisposable, IAsyncDisposable
{
    public static NullDisposable Instance { get; } = new NullDisposable();

    private NullDisposable()
    {

    }

    public void Dispose()
    {

    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public sealed class DisposableAction : IDisposable
{
    public static readonly DisposableAction Empty = new(null);

    private Action? _disposeAction;

    public DisposableAction(Action? disposeAction)
    {
        _disposeAction = disposeAction;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _disposeAction, null)?.Invoke();
    }
}
