namespace Incursa.Qlog;

internal sealed class QlogCaptureSubscription : IDisposable, IAsyncDisposable
{
    private readonly Action<QlogCaptureSubscription> unregister;
    private readonly QlogCaptureObserverExecutor executor;
    private bool disposed;

    public QlogCaptureSubscription(
        Action<QlogCaptureSubscription> unregister,
        IQlogCaptureObserver observer,
        QlogCaptureDispatchOptions? dispatchOptions = null)
    {
        this.unregister = unregister ?? throw new ArgumentNullException(nameof(unregister));
        executor = new QlogCaptureObserverExecutor(observer, dispatchOptions);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        unregister(this);
        await executor.DisposeAsync().ConfigureAwait(false);
    }

    internal void Enqueue(QlogCapturedEvent capturedEvent)
    {
        executor.Enqueue(capturedEvent);
    }

    internal ValueTask CompleteSessionAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken)
    {
        return executor.CompleteSessionAsync(sessionSnapshot, cancellationToken);
    }

    internal ValueTask FlushAsync(CancellationToken cancellationToken)
    {
        return executor.FlushAsync(cancellationToken);
    }
}
