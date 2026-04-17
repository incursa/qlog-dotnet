using System.Collections.ObjectModel;

namespace Incursa.Qlog;

internal sealed class QlogCapturedEvent
{
    private QlogCapturedEvent(
        QlogCaptureSessionSnapshot session,
        long sequence,
        double time,
        string name,
        IReadOnlyDictionary<string, QlogValue> data,
        string? tuple,
        string? timeFormat,
        string? groupId,
        IReadOnlyDictionary<string, QlogValue>? systemInfo,
        IReadOnlyDictionary<string, QlogValue> extensionData)
    {
        Session = session;
        Sequence = sequence;
        Time = time;
        Name = name;
        Data = data;
        Tuple = tuple;
        TimeFormat = timeFormat;
        GroupId = groupId;
        SystemInfo = systemInfo;
        ExtensionData = extensionData;
    }

    public QlogCaptureSessionSnapshot Session { get; }

    public long Sequence { get; }

    public double Time { get; }

    public string Name { get; }

    public IReadOnlyDictionary<string, QlogValue> Data { get; }

    public string? Tuple { get; }

    public string? TimeFormat { get; }

    public string? GroupId { get; }

    public IReadOnlyDictionary<string, QlogValue>? SystemInfo { get; }

    public IReadOnlyDictionary<string, QlogValue> ExtensionData { get; }

    public static QlogCapturedEvent Create(
        QlogCaptureSessionSnapshot session,
        long sequence,
        QlogEvent sourceEvent)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(sourceEvent);

        Dictionary<string, QlogValue> data = new(sourceEvent.Data, StringComparer.Ordinal);
        Dictionary<string, QlogValue>? systemInfo = sourceEvent.SystemInfo is null
            ? null
            : new Dictionary<string, QlogValue>(sourceEvent.SystemInfo, StringComparer.Ordinal);
        Dictionary<string, QlogValue> extensionData = new(sourceEvent.ExtensionData, StringComparer.Ordinal);

        return new(
            session,
            sequence,
            sourceEvent.Time,
            sourceEvent.Name,
            new ReadOnlyDictionary<string, QlogValue>(data),
            sourceEvent.Tuple,
            sourceEvent.TimeFormat,
            sourceEvent.GroupId,
            systemInfo is null ? null : new ReadOnlyDictionary<string, QlogValue>(systemInfo),
            new ReadOnlyDictionary<string, QlogValue>(extensionData));
    }

    public QlogEvent ToQlogEvent()
    {
        QlogEvent clone = new()
        {
            Time = Time,
            Name = Name,
            Tuple = Tuple,
            TimeFormat = TimeFormat,
            GroupId = GroupId,
            SystemInfo = SystemInfo is null
                ? null
                : new Dictionary<string, QlogValue>(SystemInfo, StringComparer.Ordinal),
        };

        foreach (KeyValuePair<string, QlogValue> entry in Data)
        {
            clone.Data[entry.Key] = entry.Value;
        }

        foreach (KeyValuePair<string, QlogValue> entry in ExtensionData)
        {
            clone.ExtensionData[entry.Key] = entry.Value;
        }

        return clone;
    }
}
