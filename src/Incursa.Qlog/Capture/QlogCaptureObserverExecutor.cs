using System.Threading.Channels;

namespace Incursa.Qlog;

internal sealed class QlogCaptureObserverExecutor
{
    private readonly IQlogCaptureObserver observer;
    private readonly Channel<Command> channel;
    private readonly Task processor;
    private Exception? terminalFailure;
    private bool disposed;

    public QlogCaptureObserverExecutor(IQlogCaptureObserver observer, QlogCaptureDispatchOptions? dispatchOptions = null)
    {
        this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        channel = CreateChannel(dispatchOptions ?? QlogCaptureDispatchOptions.Default);
        processor = ProcessCommandsAsync();
    }

    public void Enqueue(QlogCapturedEvent capturedEvent)
    {
        ArgumentNullException.ThrowIfNull(capturedEvent);

        if (disposed || terminalFailure is not null)
        {
            return;
        }

        channel.Writer.TryWrite(new CaptureCommand(capturedEvent));
    }

    public ValueTask CompleteSessionAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sessionSnapshot);
        return EnqueueAndAwaitAsync(new CompleteSessionCommand(sessionSnapshot), cancellationToken);
    }

    public ValueTask FlushAsync(CancellationToken cancellationToken)
    {
        return EnqueueAndAwaitAsync(new FlushCommand(), cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        channel.Writer.TryComplete();
        await processor.ConfigureAwait(false);

        switch (observer)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    private async ValueTask EnqueueAndAwaitAsync(Command command, CancellationToken cancellationToken)
    {
        if (terminalFailure is not null)
        {
            throw terminalFailure;
        }

        if (disposed)
        {
            throw new ObjectDisposedException(nameof(QlogCaptureObserverExecutor));
        }

        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        command.SetCompletion(completion);

        try
        {
            if (!channel.Writer.TryWrite(command))
            {
                await channel.Writer.WriteAsync(command, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (ChannelClosedException)
        {
            if (terminalFailure is not null)
            {
                throw terminalFailure;
            }

            throw new InvalidOperationException("The capture observer executor is not accepting more work.");
        }

        if (cancellationToken.CanBeCanceled)
        {
            await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await completion.Task.ConfigureAwait(false);
    }

    private async Task ProcessCommandsAsync()
    {
        await foreach (Command command in channel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            if (terminalFailure is not null)
            {
                command.TrySetException(terminalFailure);
                continue;
            }

            try
            {
                switch (command)
                {
                    case CaptureCommand capture:
                        await observer.OnCapturedAsync(capture.CapturedEvent, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case CompleteSessionCommand completeSession:
                        await observer.OnSessionCompletedAsync(completeSession.SessionSnapshot, CancellationToken.None).ConfigureAwait(false);
                        completeSession.TrySetResult();
                        break;
                    case FlushCommand flush:
                        flush.TrySetResult();
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported capture command type '{command.GetType().FullName}'.");
                }
            }
            catch (Exception ex)
            {
                terminalFailure = ex;
                command.TrySetException(ex);
            }
        }
    }

    private static Channel<Command> CreateChannel(QlogCaptureDispatchOptions dispatchOptions)
    {
        ArgumentNullException.ThrowIfNull(dispatchOptions);

        return dispatchOptions.BackpressureMode switch
        {
            QlogCaptureBackpressureMode.BoundedDropNewest => Channel.CreateBounded<Command>(new BoundedChannelOptions(dispatchOptions.ResolveBoundedCapacity())
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait,
            }),
            QlogCaptureBackpressureMode.Unbounded => Channel.CreateUnbounded<Command>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            }),
            _ => throw new InvalidOperationException($"Unsupported capture backpressure mode '{dispatchOptions.BackpressureMode}'."),
        };
    }

    private abstract class Command
    {
        private TaskCompletionSource? completion;

        public void SetCompletion(TaskCompletionSource completionSource)
        {
            completion = completionSource;
        }

        public void TrySetResult()
        {
            completion?.TrySetResult();
        }

        public void TrySetException(Exception exception)
        {
            completion?.TrySetException(exception);
        }
    }

    private sealed class CaptureCommand : Command
    {
        public CaptureCommand(QlogCapturedEvent capturedEvent)
        {
            CapturedEvent = capturedEvent;
        }

        public QlogCapturedEvent CapturedEvent { get; }
    }

    private sealed class CompleteSessionCommand : Command
    {
        public CompleteSessionCommand(QlogCaptureSessionSnapshot sessionSnapshot)
        {
            SessionSnapshot = sessionSnapshot;
        }

        public QlogCaptureSessionSnapshot SessionSnapshot { get; }
    }

    private sealed class FlushCommand : Command
    {
    }
}
