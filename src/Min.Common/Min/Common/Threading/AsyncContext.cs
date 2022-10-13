using System.Collections.Concurrent;
using System.Diagnostics;

namespace Min.Common.Threading;

/// <summary>
/// Provides a context for asynchronous operations. This class is threadsafe.
/// </summary>
/// <remarks>
/// <para><see cref="Execute()"/> may only be called once. After <see cref="Execute()"/> returns, the async context should be disposed.</para>
/// </remarks>
[DebuggerDisplay("Id = {Id}, OperationCount = {_outstandingOperations}")]
[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerStepThrough]
public sealed class AsyncContext : IDisposable
{
    /// <summary>
    /// The queue holding the actions to run.
    /// </summary>
    private readonly TaskQueue _queue;

    /// <summary>
    /// The <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>.
    /// </summary>
    private readonly AsyncContextSynchronizationContext _synchronizationContext;

    /// <summary>
    /// The <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>.
    /// </summary>
    private readonly AsyncContextTaskScheduler _taskScheduler;

    /// <summary>
    /// The <see cref="TaskFactory"/> for this <see cref="AsyncContext"/>.
    /// </summary>
    private readonly TaskFactory _taskFactory;

    /// <summary>
    /// The number of outstanding operations, including actions in the queue.
    /// </summary>
    private int _outstandingOperations;

    /// <summary>
    /// The child thread.
    /// </summary>
    private readonly Task _thread;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncContext"/> class. This is an advanced operation; most people should use one of the static <c>Run</c> methods instead.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public AsyncContext()
    {
        _queue = new TaskQueue();
        _synchronizationContext = new AsyncContextSynchronizationContext(this);
        _taskScheduler = new AsyncContextTaskScheduler(this);
        _taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.HideScheduler, TaskContinuationOptions.HideScheduler, _taskScheduler);
    }

    /// <summary>
    /// Gets a semi-unique identifier for this asynchronous context. This is the same identifier as the context's <see cref="TaskScheduler"/>.
    /// </summary>
    public int Id => _taskScheduler.Id;

    /// <summary>
    /// Increments the outstanding asynchronous operation count.
    /// </summary>
    private void OperationStarted()
    {
        var newCount = Interlocked.Increment(ref _outstandingOperations);
    }

    /// <summary>
    /// Decrements the outstanding asynchronous operation count.
    /// </summary>
    private void OperationCompleted()
    {
        var newCount = Interlocked.Decrement(ref _outstandingOperations);
        if (newCount == 0)
            _queue.CompleteAdding();
    }

    /// <summary>
    /// Queues a task for execution by <see cref="Execute"/>. If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
    /// </summary>
    /// <param name="task">The task to queue. May not be <c>null</c>.</param>
    /// <param name="propagateExceptions">A value indicating whether exceptions on this task should be propagated out of the main loop.</param>
    private void Enqueue(Task task, bool propagateExceptions)
    {
        OperationStarted();
        task.ContinueWith(_ => OperationCompleted(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, _taskScheduler);
        _queue.TryAdd(task, propagateExceptions);

        // If we fail to add to the queue, just drop the Task. This is the same behavior as the TaskScheduler.FromCurrentSynchronizationContext(WinFormsSynchronizationContext).
    }

    /// <summary>
    /// Disposes all resources used by this class. This method should NOT be called while <see cref="Execute"/> is executing.
    /// </summary>
    public void Dispose()
    {
        _queue.Dispose();
    }

    /// <summary>
    /// Executes all queued actions. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from tasks that are supposed to propagate errors.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void Execute()
    {
        SynchronizationContextSwitcher.ApplyContext(_synchronizationContext, () =>
        {
            var tasks = _queue.GetConsumingEnumerable();
            foreach (var task in tasks)
            {
                _taskScheduler.DoTryExecuteTask(task.Item1);

                // Propagate exception if necessary.
                if (task.Item2)
                    task.Item1.WaitAndUnwrapException();
            }
        });
    }

    /// <summary>
    /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task.
    /// </summary>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static void Run(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (var context = new AsyncContext())
        {
            var task = context._taskFactory.Run(action);
            context.Execute();
            task.WaitAndUnwrapException();
        }
    }

    /// <summary>
    /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task. This method will unwrap and propagate errors from the task.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static TResult Run<TResult>(Func<TResult> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (var context = new AsyncContext())
        {
            var task = context._taskFactory.Run(action);
            context.Execute();
            return task.WaitAndUnwrapException();
        }
    }

    /// <summary>
    /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task proxy.
    /// </summary>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static void Run(Func<Task> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (var context = new AsyncContext())
        {
            context.OperationStarted();
            var task = context._taskFactory.Run(action).ContinueWith(t =>
            {
                context.OperationCompleted();
                t.WaitAndUnwrapException();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
            context.Execute();
            task.WaitAndUnwrapException();
        }
    }

    /// <summary>
    /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task proxy. This method will unwrap and propagate errors from the task proxy.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static TResult Run<TResult>(Func<Task<TResult>> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (var context = new AsyncContext())
        {
            context.OperationStarted();
            var task = context._taskFactory.Run(action).ContinueWith(t =>
            {
                context.OperationCompleted();
                return t.WaitAndUnwrapException();
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
            context.Execute();
            return task.WaitAndUnwrapException();
        }
    }

    /// <summary>
    /// Gets the current <see cref="AsyncContext"/> for this thread, or <c>null</c> if this thread is not currently running in an <see cref="AsyncContext"/>.
    /// </summary>
    public static AsyncContext? Current
    {
        get
        {
            var syncContext = SynchronizationContext.Current as AsyncContextSynchronizationContext;
            return syncContext?.Context;
        }
    }

    /// <summary>
    /// Gets the <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>. From inside <see cref="Execute"/>, this value is always equal to <see cref="System.Threading.SynchronizationContext.Current"/>.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public SynchronizationContext SynchronizationContext => _synchronizationContext;

    /// <summary>
    /// Gets the <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>. From inside <see cref="Execute"/>, this value is always equal to <see cref="TaskScheduler.Current"/>.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public TaskScheduler Scheduler => _taskScheduler;


    /// <summary>
    /// Gets the <see cref="TaskFactory"/> for this <see cref="AsyncContext"/>. Note that this factory has the <see cref="TaskCreationOptions.HideScheduler"/> option set. Be careful with async delegates; you may need to call <see cref="M:System.Threding.SynchronizationContext.OperationStarted"/> and <see cref="M:System.Threading.SynchronizationContext.OperationCompleted"/> to prevent early termination of this <see cref="AsyncContext"/>.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public TaskFactory Factory => _taskFactory;

    [DebuggerNonUserCode]
    internal sealed class DebugView
    {
        private readonly AsyncContext _context;

        public DebugView(AsyncContext context)
        {
            _context = context;
        }

        public TaskScheduler TaskScheduler => _context._taskScheduler;
    }

    /// <summary>
    /// A blocking queue.
    /// </summary>
    private sealed class TaskQueue : IDisposable
    {
        /// <summary>
        /// The underlying blocking collection.
        /// </summary>
        private readonly BlockingCollection<Tuple<Task, bool>> _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskQueue"/> class.
        /// </summary>
        public TaskQueue()
        {
            _queue = new BlockingCollection<Tuple<Task, bool>>();
        }

        /// <summary>
        /// Gets a blocking enumerable that removes items from the queue. This enumerable only completes after <see cref="CompleteAdding"/> has been called.
        /// </summary>
        /// <returns>A blocking enumerable that removes items from the queue.</returns>
        public IEnumerable<Tuple<Task, bool>> GetConsumingEnumerable()
        {
            return _queue.GetConsumingEnumerable();
        }

        /// <summary>
        /// Generates an enumerable of <see cref="Task"/> instances currently queued to the scheduler waiting to be executed.
        /// </summary>
        /// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.</returns>
        [DebuggerNonUserCode]
        internal IEnumerable<Task> GetScheduledTasks()
        {
            foreach (var item in _queue)
                yield return item.Item1;
        }

        /// <summary>
        /// Attempts to add the item to the queue. If the queue has been marked as complete for adding, this method returns <c>false</c>.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="propagateExceptions">A value indicating whether exceptions on this task should be propagated out of the main loop.</param>
        public bool TryAdd(Task item, bool propagateExceptions)
        {
            try
            {
                return _queue.TryAdd(Tuple.Create(item, propagateExceptions));
            }
            catch (InvalidOperationException)
            {
                // vexing exception
                return false;
            }
        }

        /// <summary>
        /// Marks the queue as complete for adding, allowing the enumerator returned from <see cref="GetConsumingEnumerable"/> to eventually complete. This method may be called several times.
        /// </summary>
        public void CompleteAdding()
        {
            _queue.CompleteAdding();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _queue.Dispose();
        }
    }

    /// <summary>
    /// The <see cref="SynchronizationContext"/> implementation used by <see cref="AsyncContext"/>.
    /// </summary>
    private sealed class AsyncContextSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// The async context.
        /// </summary>
        private readonly AsyncContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextSynchronizationContext"/> class.
        /// </summary>
        /// <param name="context">The async context.</param>
        public AsyncContextSynchronizationContext(AsyncContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the async context.
        /// </summary>
        public AsyncContext Context
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// Dispatches an asynchronous message to the async context. If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
        /// </summary>
        /// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            _context.Enqueue(_context._taskFactory.Run(() => d(state)), true);
        }

        /// <summary>
        /// Dispatches an asynchronous message to the async context, and waits for it to complete.
        /// </summary>
        /// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Send(SendOrPostCallback d, object state)
        {
            if (AsyncContext.Current == _context)
            {
                d(state);
            }
            else
            {
                var task = _context._taskFactory.Run(() => d(state));
                task.WaitAndUnwrapException();
            }
        }

        /// <summary>
        /// Responds to the notification that an operation has started by incrementing the outstanding asynchronous operation count.
        /// </summary>
        public override void OperationStarted()
        {
            _context.OperationStarted();
        }

        /// <summary>
        /// Responds to the notification that an operation has completed by decrementing the outstanding asynchronous operation count.
        /// </summary>
        public override void OperationCompleted()
        {
            _context.OperationCompleted();
        }

        /// <summary>
        /// Creates a copy of the synchronization context.
        /// </summary>
        /// <returns>A new <see cref="SynchronizationContext"/> object.</returns>
        public override SynchronizationContext CreateCopy()
        {
            return new AsyncContextSynchronizationContext(_context);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _context.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance. It is considered equal if it refers to the same underlying async context as this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as AsyncContextSynchronizationContext;
            if (other == null)
                return false;
            return (_context == other._context);
        }
    }

    /// <summary>
    /// A task scheduler which schedules tasks to an async context.
    /// </summary>
    private sealed class AsyncContextTaskScheduler : TaskScheduler
    {
        /// <summary>
        /// The async context for this task scheduler.
        /// </summary>
        private readonly AsyncContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextTaskScheduler"/> class.
        /// </summary>
        /// <param name="context">The async context for this task scheduler. May not be <c>null</c>.</param>
        public AsyncContextTaskScheduler(AsyncContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates an enumerable of <see cref="Task"/> instances currently queued to the scheduler waiting to be executed.
        /// </summary>
        /// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.</returns>
        [System.Diagnostics.DebuggerNonUserCode]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _context._queue.GetScheduledTasks();
        }

        /// <summary>
        /// Queues a <see cref="Task"/> to the scheduler. If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be queued.</param>
        protected override void QueueTask(Task task)
        {
            _context.Enqueue(task, false);
        }

        /// <summary>
        /// Determines whether the provided <see cref="Task"/> can be executed synchronously in this call, and if it can, executes it.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">A Boolean denoting whether or not task has previously been queued. If this parameter is True, then the task may have been previously queued (scheduled); if False, then the task is known not to have been queued, and this call is being made in order to execute the task inline without queuing it.</param>
        /// <returns>A Boolean value indicating whether the task was executed inline.</returns>
        /// <exception cref="System.InvalidOperationException">The <paramref name="task"/> was already executed.</exception>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (AsyncContext.Current == _context) && TryExecuteTask(task);
        }

        /// <summary>
        /// Indicates the maximum concurrency level this <see cref="TaskScheduler"/> is able to support.
        /// </summary>
        public override int MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        /// <summary>
        /// Exposes the base <see cref="TaskScheduler.TryExecuteTask"/> method.
        /// </summary>
        /// <param name="task">The task to attempt to execute.</param>
        public void DoTryExecuteTask(Task task)
        {
            TryExecuteTask(task);
        }
    }

    /// <summary>
    /// Utility class for temporarily switching <see cref="SynchronizationContext"/> implementations.
    /// </summary>
    private sealed class SynchronizationContextSwitcher : SingleDisposable<object>
    {
        /// <summary>
        /// The previous <see cref="SynchronizationContext"/>.
        /// </summary>
        private readonly SynchronizationContext? _oldContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextSwitcher"/> class, installing the new <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="newContext">The new <see cref="SynchronizationContext"/>. This can be <c>null</c> to remove an existing <see cref="SynchronizationContext"/>.</param>
        private SynchronizationContextSwitcher(SynchronizationContext? newContext)
            : base(new object())
        {
            _oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        /// <summary>
        /// Restores the old <see cref="SynchronizationContext"/>.
        /// </summary>
        protected override void Dispose(object context)
        {
            SynchronizationContext.SetSynchronizationContext(_oldContext);
        }

        /// <summary>
        /// Executes a synchronous delegate without the current <see cref="SynchronizationContext"/>. The current context is restored when this function returns.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        public static void NoContext(Action action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(null))
                action();
        }

        /// <summary>
        /// Executes a synchronous or asynchronous delegate without the current <see cref="SynchronizationContext"/>. The current context is restored when this function synchronously returns.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        public static T NoContext<T>(Func<T> action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(null))
                return action();
        }

        /// <summary>
        /// Executes a synchronous delegate with the specified <see cref="SynchronizationContext"/> as "current". The previous current context is restored when this function returns.
        /// </summary>
        /// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static void ApplyContext(SynchronizationContext context, Action action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(context))
                action();
        }

        /// <summary>
        /// Executes a synchronous or asynchronous delegate without the specified <see cref="SynchronizationContext"/> as "current". The previous current context is restored when this function synchronously returns.
        /// </summary>
        /// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static T ApplyContext<T>(SynchronizationContext context, Func<T> action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(context))
                return action();
        }
    }
}
