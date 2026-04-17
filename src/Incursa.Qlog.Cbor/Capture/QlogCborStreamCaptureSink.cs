namespace Incursa.Qlog;

internal sealed class QlogCborStreamCaptureSink : IQlogCaptureObserver, IAsyncDisposable
{
    private readonly Stream stream;
    private readonly bool leaveOpen;
    private readonly Dictionary<string, List<QlogCapturedEvent>> pendingEvents = new(StringComparer.Ordinal);
    private QlogFile? containedFile;

    public QlogCborStreamCaptureSink(Stream stream, bool leaveOpen = true)
    {
        this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        this.leaveOpen = leaveOpen;
    }

    public ValueTask OnCapturedAsync(QlogCapturedEvent capturedEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(capturedEvent);

        if (!pendingEvents.TryGetValue(capturedEvent.Session.SessionId, out List<QlogCapturedEvent>? events))
        {
            events = new List<QlogCapturedEvent>();
            pendingEvents.Add(capturedEvent.Session.SessionId, events);
        }

        events.Add(capturedEvent);
        return ValueTask.CompletedTask;
    }

    public async ValueTask OnSessionCompletedAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sessionSnapshot);

        List<QlogCapturedEvent> capturedEvents = pendingEvents.TryGetValue(sessionSnapshot.SessionId, out List<QlogCapturedEvent>? events)
            ? events
            : new List<QlogCapturedEvent>();
        pendingEvents.Remove(sessionSnapshot.SessionId);

        Serialization.Cbor.QlogCborSerializer.Serialize(stream, AppendContainedSession(sessionSnapshot, capturedEvents));
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (leaveOpen)
        {
            return;
        }

        await stream.DisposeAsync().ConfigureAwait(false);
    }

    private QlogFile AppendContainedSession(QlogCaptureSessionSnapshot sessionSnapshot, IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        QlogFile completedSessionFile = CreateContainedCborFile(sessionSnapshot, capturedEvents);
        if (containedFile is null)
        {
            containedFile = QlogContainedFileAggregator.AppendCompletedSession(null, completedSessionFile);
            PrepareContainedStreamForWrite(multiSessionRewrite: false);
            return containedFile;
        }

        PrepareContainedStreamForWrite(multiSessionRewrite: true);
        containedFile = QlogContainedFileAggregator.AppendCompletedSession(containedFile, completedSessionFile);
        return containedFile;
    }

    private void PrepareContainedStreamForWrite(bool multiSessionRewrite)
    {
        if (!stream.CanSeek)
        {
            if (multiSessionRewrite)
            {
                throw new InvalidOperationException("Contained CBOR capture stream sinks require a seekable stream to aggregate more than one completed session.");
            }

            return;
        }

        stream.SetLength(0);
        stream.Position = 0;
    }

    private static QlogFile CreateContainedCborFile(
        QlogCaptureSessionSnapshot sessionSnapshot,
        IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        QlogFile file = sessionSnapshot.CreateContainedFile(capturedEvents);
        file.SerializationFormat = Serialization.Cbor.QlogCborKnownValues.ContainedCborSerializationFormat;
        return file;
    }
}
