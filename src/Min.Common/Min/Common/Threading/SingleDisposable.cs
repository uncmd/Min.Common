namespace Min.Common.Threading;

/// <summary>
/// A base class for disposables that need exactly-once semantics in a thread-safe way. All disposals of this instance block until the disposal is complete.
/// </summary>
/// <typeparam name="T">The type of "context" for the derived disposable. Since the context should not be modified, strongly consider making this an immutable type.</typeparam>
/// <remarks>
/// <para>If <see cref="Dispose()"/> is called multiple times, only the first call will execute the disposal code. Other calls to <see cref="Dispose()"/> will wait for the disposal to complete.</para>
/// </remarks>
public abstract class SingleDisposable<T> : IDisposable
{
    /// <summary>
    /// The context. This is never <c>null</c>. This is empty if this instance has already been disposed (or is being disposed).
    /// </summary>
    private readonly BoundActionField<T> _context;

    private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

    /// <summary>
    /// Creates a disposable for the specified context.
    /// </summary>
    /// <param name="context">The context passed to <see cref="Dispose(T)"/>. May be <c>null</c>.</param>
    protected SingleDisposable(T context)
    {
        _context = new BoundActionField<T>(Dispose, context);
    }

    /// <summary>
    /// Whether this instance is currently disposing or has been disposed.
    /// </summary>
    public bool IsDisposeStarted => _context.IsEmpty;

    /// <summary>
    /// Whether this instance is disposed (finished disposing).
    /// </summary>
    public bool IsDisposed => _tcs.Task.IsCompleted;

    /// <summary>
    /// Whether this instance is currently disposing, but not finished yet.
    /// </summary>
    public bool IsDisposing => IsDisposeStarted && !IsDisposed;

    /// <summary>
    /// The actual disposal method, called only once from <see cref="Dispose()"/>.
    /// </summary>
    /// <param name="context">The context for the disposal operation.</param>
    protected abstract void Dispose(T context);

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    /// <remarks>
    /// <para>If <see cref="Dispose()"/> is called multiple times, only the first call will execute the disposal code. Other calls to <see cref="Dispose()"/> will wait for the disposal to complete.</para>
    /// </remarks>
    public void Dispose()
    {
        var context = _context.TryGetAndUnset();
        if (context == null)
        {
            _tcs.Task.GetAwaiter().GetResult();
            return;
        }
        try
        {
            context.Invoke();
        }
        finally
        {
            _tcs.TrySetResult(null!);
        }
    }

    /// <summary>
    /// Attempts to update the stored context. This method returns <c>false</c> if this instance has already been disposed (or is being disposed).
    /// </summary>
    /// <param name="contextUpdater">The function used to update an existing context. This may be called more than once if more than one thread attempts to simultaneously update the context.</param>
    protected bool TryUpdateContext(Func<T, T> contextUpdater) => _context.TryUpdateContext(contextUpdater);

    private sealed class BoundActionField<T>
    {
        private BoundAction? _field;

        /// <summary>
        /// Initializes the field with the specified action and context.
        /// </summary>
        /// <param name="action">The action delegate.</param>
        /// <param name="context">The context.</param>
        public BoundActionField(Action<T> action, T context)
        {
            _field = new BoundAction(action, context);
        }

        /// <summary>
        /// Whether the field is empty.
        /// </summary>
        public bool IsEmpty => Interlocked.CompareExchange(ref _field, null, null) == null;

        /// <summary>
        /// Atomically retrieves the bound action from the field and sets the field to <c>null</c>. May return <c>null</c>.
        /// </summary>
        public IBoundAction? TryGetAndUnset()
        {
            return Interlocked.Exchange(ref _field, null);
        }

        /// <summary>
        /// Attempts to update the context of the bound action stored in the field. Returns <c>false</c> if the field is <c>null</c>.
        /// </summary>
        /// <param name="contextUpdater">The function used to update an existing context. This may be called more than once if more than one thread attempts to simultaneously update the context.</param>
        public bool TryUpdateContext(Func<T, T> contextUpdater)
        {
            _ = contextUpdater ?? throw new ArgumentNullException(nameof(contextUpdater));
            while (true)
            {
                var original = Interlocked.CompareExchange(ref _field, null, null);
                if (original == null)
                    return false;
                var updatedContext = new BoundAction(original, contextUpdater);
                var result = Interlocked.CompareExchange(ref _field, updatedContext, original);
                if (ReferenceEquals(original, result))
                    return true;
            }
        }

        /// <summary>
        /// An action delegate bound with its context.
        /// </summary>
        public interface IBoundAction
        {
            /// <summary>
            /// Executes the action. This should only be done after the bound action is retrieved from a field by <see cref="TryGetAndUnset"/>.
            /// </summary>
            void Invoke();
        }

        private sealed class BoundAction : IBoundAction
        {
            private readonly Action<T> _action;
            private readonly T _context;

            public BoundAction(Action<T> action, T context)
            {
                _action = action;
                _context = context;
            }

            public BoundAction(BoundAction originalBoundAction, Func<T, T> contextUpdater)
            {
                _action = originalBoundAction._action;
                _context = contextUpdater(originalBoundAction._context);
            }

            public void Invoke() => _action?.Invoke(_context);
        }
    }
}
