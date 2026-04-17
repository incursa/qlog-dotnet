using System.Runtime.ExceptionServices;

namespace Incursa.Qlog;

internal sealed class QlogCaptureCoordinator : IAsyncDisposable
{
    private readonly object gate = new();
    private readonly List<QlogCaptureSubscription> processObservers = new();
    private readonly Dictionary<string, List<QlogCaptureSubscription>> sessionObservers = new(StringComparer.Ordinal);
    private readonly HashSet<QlogCaptureSubscription> allSubscriptions = new();
    private bool disposed;

    public QlogCaptureSubscription RegisterProcessObserver(IQlogCaptureObserver observer)
    {
        return RegisterProcessObserver(observer, QlogCaptureDispatchOptions.Default);
    }

    internal QlogCaptureSubscription RegisterProcessObserver(IQlogCaptureObserver observer, QlogCaptureDispatchOptions dispatchOptions)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(dispatchOptions);

        QlogCaptureSubscription subscription = new(Unregister, observer, dispatchOptions);
        lock (gate)
        {
            allSubscriptions.Add(subscription);
            processObservers.Add(subscription);
        }

        return subscription;
    }

    public QlogCaptureSubscription RegisterSessionObserver(QlogCaptureSession session, IQlogCaptureObserver observer)
    {
        return RegisterSessionObserver(session, observer, QlogCaptureDispatchOptions.Default);
    }

    internal QlogCaptureSubscription RegisterSessionObserver(
        QlogCaptureSession session,
        IQlogCaptureObserver observer,
        QlogCaptureDispatchOptions dispatchOptions)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(dispatchOptions);

        lock (session.Gate)
        {
            session.ThrowIfCompleted();
        }

        QlogCaptureSubscription subscription = new(Unregister, observer, dispatchOptions);
        lock (gate)
        {
            allSubscriptions.Add(subscription);
            if (!sessionObservers.TryGetValue(session.SessionId, out List<QlogCaptureSubscription>? observers))
            {
                observers = new List<QlogCaptureSubscription>();
                sessionObservers.Add(session.SessionId, observers);
            }

            observers.Add(subscription);
        }

        return subscription;
    }

    public void Capture(QlogCaptureSession session, QlogEvent qlogEvent)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(qlogEvent);

        QlogCapturedEvent capturedEvent;
        QlogCaptureSubscription[] targets;
        lock (session.Gate)
        {
            session.ThrowIfCompleted();
            capturedEvent = QlogCapturedEvent.Create(session.Snapshot, session.NextSequence(), qlogEvent);
            targets = GetTargets(session.SessionId);
        }

        foreach (QlogCaptureSubscription target in targets)
        {
            target.Enqueue(capturedEvent);
        }
    }

    public async ValueTask CompleteSessionAsync(QlogCaptureSession session, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(session);

        QlogCaptureSubscription[] targets;
        lock (session.Gate)
        {
            session.MarkCompleted();
            targets = GetTargets(session.SessionId);
        }

        lock (gate)
        {
            sessionObservers.Remove(session.SessionId);
        }

        await AwaitAllAsync(targets.Select(target => target.CompleteSessionAsync(session.Snapshot, cancellationToken))).ConfigureAwait(false);
    }

    public ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        QlogCaptureSubscription[] subscriptions;
        lock (gate)
        {
            subscriptions = allSubscriptions.ToArray();
        }

        return AwaitAllAsync(subscriptions.Select(subscription => subscription.FlushAsync(cancellationToken)));
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        QlogCaptureSubscription[] subscriptions;
        lock (gate)
        {
            subscriptions = allSubscriptions.ToArray();
            allSubscriptions.Clear();
            processObservers.Clear();
            sessionObservers.Clear();
        }

        await AwaitAllAsync(subscriptions.Select(subscription => subscription.DisposeAsync())).ConfigureAwait(false);
    }

    private QlogCaptureSubscription[] GetTargets(string sessionId)
    {
        lock (gate)
        {
            IEnumerable<QlogCaptureSubscription> sessionTargets = sessionObservers.TryGetValue(sessionId, out List<QlogCaptureSubscription>? observers)
                ? observers
                : Array.Empty<QlogCaptureSubscription>();
            return processObservers.Concat(sessionTargets).ToArray();
        }
    }

    private void Unregister(QlogCaptureSubscription subscription)
    {
        lock (gate)
        {
            allSubscriptions.Remove(subscription);
            processObservers.Remove(subscription);

            foreach (KeyValuePair<string, List<QlogCaptureSubscription>> entry in sessionObservers.ToArray())
            {
                entry.Value.Remove(subscription);
                if (entry.Value.Count == 0)
                {
                    sessionObservers.Remove(entry.Key);
                }
            }
        }
    }

    private static async ValueTask AwaitAllAsync(IEnumerable<ValueTask> operations)
    {
        Task[] tasks = operations.Select(operation => operation.AsTask()).ToArray();
        if (tasks.Length == 0)
        {
            return;
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch
        {
            AggregateException aggregate = tasks
                .Where(static task => task.IsFaulted && task.Exception is not null)
                .Select(static task => task.Exception!)
                .Aggregate(
                    new AggregateException(),
                    static (current, next) => new AggregateException(current.InnerExceptions.Concat(next.InnerExceptions)));

            if (aggregate.InnerExceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(aggregate.InnerExceptions[0]).Throw();
            }

            throw aggregate;
        }
    }
}
