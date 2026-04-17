using Incursa.Qlog.Serialization;

namespace Incursa.Qlog;

internal sealed class QlogCaptureSessionSnapshot
{
    private readonly string? fileTitle;
    private readonly string? fileDescription;
    private readonly Dictionary<string, QlogValue> fileExtensionData;
    private readonly QlogTrace traceHeader;

    private QlogCaptureSessionSnapshot(
        string sessionId,
        string? fileTitle,
        string? fileDescription,
        Dictionary<string, QlogValue> fileExtensionData,
        QlogTrace traceHeader)
    {
        SessionId = sessionId;
        this.fileTitle = fileTitle;
        this.fileDescription = fileDescription;
        this.fileExtensionData = fileExtensionData;
        this.traceHeader = traceHeader;
    }

    public string SessionId { get; }

    public static QlogCaptureSessionSnapshot Create(
        string sessionId,
        QlogTrace traceHeader,
        string? fileTitle,
        string? fileDescription,
        IEnumerable<KeyValuePair<string, QlogValue>>? fileExtensionData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(traceHeader);

        QlogTrace frozenTraceHeader = QlogCaptureModelCloner.CloneTraceHeader(traceHeader);
        Dictionary<string, QlogValue> frozenFileExtensionData = new(StringComparer.Ordinal);
        if (fileExtensionData is not null)
        {
            foreach (KeyValuePair<string, QlogValue> entry in fileExtensionData)
            {
                frozenFileExtensionData[entry.Key] = entry.Value;
            }
        }

        QlogCaptureSessionSnapshot snapshot = new(
            sessionId,
            fileTitle,
            fileDescription,
            frozenFileExtensionData,
            frozenTraceHeader);

        QlogSerializationCore.ValidateContainedFile(snapshot.CreateContainedFile(Array.Empty<QlogCapturedEvent>()));
        QlogSerializationCore.ValidateSequentialFile(snapshot.CreateSequentialFile(Array.Empty<QlogCapturedEvent>()));
        return snapshot;
    }

    public QlogFile CreateContainedFile(IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        ArgumentNullException.ThrowIfNull(capturedEvents);

        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.ContainedFileSchemaUri,
            SerializationFormat = QlogKnownValues.ContainedJsonSerializationFormat,
            Title = fileTitle,
            Description = fileDescription,
        };

        foreach (KeyValuePair<string, QlogValue> entry in fileExtensionData)
        {
            file.ExtensionData[entry.Key] = entry.Value;
        }

        file.Traces.Add(CreateTrace(capturedEvents));
        return file;
    }

    public QlogFile CreateSequentialFile(IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        ArgumentNullException.ThrowIfNull(capturedEvents);

        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
            Title = fileTitle,
            Description = fileDescription,
        };

        foreach (KeyValuePair<string, QlogValue> entry in fileExtensionData)
        {
            file.ExtensionData[entry.Key] = entry.Value;
        }

        file.Traces.Add(CreateTrace(capturedEvents));
        return file;
    }

    private QlogTrace CreateTrace(IEnumerable<QlogCapturedEvent> capturedEvents)
    {
        QlogTrace trace = QlogCaptureModelCloner.CloneTraceHeader(traceHeader);
        foreach (QlogCapturedEvent capturedEvent in capturedEvents.OrderBy(static capturedEvent => capturedEvent.Sequence))
        {
            trace.Events.Add(capturedEvent.ToQlogEvent());
        }

        return trace;
    }
}
