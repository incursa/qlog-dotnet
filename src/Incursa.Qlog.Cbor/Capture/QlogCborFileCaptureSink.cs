namespace Incursa.Qlog;

internal sealed class QlogCborFileCaptureSink : IQlogCaptureObserver
{
    private readonly string filePath;
    private readonly Dictionary<string, List<QlogCapturedEvent>> pendingEvents = new(StringComparer.Ordinal);
    private QlogFile? containedFile;

    public QlogCborFileCaptureSink(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        this.filePath = filePath;
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

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = new(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read);

        Serialization.Cbor.QlogCborSerializer.Serialize(stream, AppendContainedSession(sessionSnapshot, capturedEvents));
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private QlogFile AppendContainedSession(QlogCaptureSessionSnapshot sessionSnapshot, IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        QlogFile completedSessionFile = CreateContainedCborFile(sessionSnapshot, capturedEvents);
        containedFile = QlogContainedFileAggregator.AppendCompletedSession(containedFile, completedSessionFile);
        return containedFile;
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
