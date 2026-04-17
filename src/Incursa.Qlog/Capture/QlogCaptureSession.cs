namespace Incursa.Qlog;

internal sealed class QlogCaptureSession
{
    private long nextSequence;
    private bool completed;

    public QlogCaptureSession(
        string sessionId,
        QlogTrace traceHeader,
        string? fileTitle = null,
        string? fileDescription = null,
        IEnumerable<KeyValuePair<string, QlogValue>>? fileExtensionData = null)
    {
        Snapshot = QlogCaptureSessionSnapshot.Create(
            sessionId,
            traceHeader,
            fileTitle,
            fileDescription,
            fileExtensionData);
    }

    public string SessionId => Snapshot.SessionId;

    internal object Gate { get; } = new();

    internal QlogCaptureSessionSnapshot Snapshot { get; }

    internal long NextSequence()
    {
        nextSequence++;
        return nextSequence;
    }

    internal void ThrowIfCompleted()
    {
        if (completed)
        {
            throw new InvalidOperationException($"Capture session '{SessionId}' has already completed.");
        }
    }

    internal void MarkCompleted()
    {
        ThrowIfCompleted();
        completed = true;
    }
}
