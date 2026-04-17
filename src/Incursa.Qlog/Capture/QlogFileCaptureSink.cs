using Incursa.Qlog.Serialization.Json;

namespace Incursa.Qlog;

internal sealed class QlogFileCaptureSink : IQlogCaptureObserver
{
    private readonly string filePath;
    private readonly QlogCaptureSinkFormat format;
    private readonly Dictionary<string, List<QlogCapturedEvent>> pendingEvents = new(StringComparer.Ordinal);
    private QlogFile? containedFile;

    public QlogFileCaptureSink(string filePath, QlogCaptureSinkFormat format = QlogCaptureSinkFormat.SequentialJsonTextSequences)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        this.filePath = filePath;
        this.format = format;
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
            format == QlogCaptureSinkFormat.ContainedJson ? FileMode.Create : FileMode.Append,
            FileAccess.Write,
            FileShare.Read);

        switch (format)
        {
            case QlogCaptureSinkFormat.ContainedJson:
                QlogJsonSerializer.Serialize(stream, AppendContainedSession(sessionSnapshot, capturedEvents), indented: false);
                break;
            case QlogCaptureSinkFormat.SequentialJsonTextSequences:
                QlogJsonTextSequenceSerializer.Serialize(stream, sessionSnapshot.CreateSequentialFile(capturedEvents), indented: false);
                break;
            default:
                throw new InvalidOperationException($"Unsupported capture sink format '{format}'.");
        }

        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private QlogFile AppendContainedSession(QlogCaptureSessionSnapshot sessionSnapshot, IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        QlogFile completedSessionFile = sessionSnapshot.CreateContainedFile(capturedEvents);
        containedFile = QlogContainedFileAggregator.AppendCompletedSession(containedFile, completedSessionFile);
        return containedFile;
    }
}
