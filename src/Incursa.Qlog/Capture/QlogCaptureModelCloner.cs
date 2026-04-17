namespace Incursa.Qlog;

internal static class QlogCaptureModelCloner
{
    public static QlogTrace CloneTraceHeader(QlogTrace source)
    {
        ArgumentNullException.ThrowIfNull(source);

        QlogTrace clone = new()
        {
            Title = source.Title,
            Description = source.Description,
            CommonFields = CloneCommonFields(source.CommonFields),
            VantagePoint = CloneVantagePoint(source.VantagePoint),
        };

        foreach (Uri eventSchema in source.EventSchemas)
        {
            clone.EventSchemas.Add(new Uri(eventSchema.OriginalString, UriKind.Absolute));
        }

        CopyEntries(source.ExtensionData, clone.ExtensionData);
        return clone;
    }

    public static QlogEvent CloneEvent(QlogEvent source)
    {
        ArgumentNullException.ThrowIfNull(source);

        QlogEvent clone = new()
        {
            Time = source.Time,
            Name = source.Name,
            Tuple = source.Tuple,
            TimeFormat = source.TimeFormat,
            GroupId = source.GroupId,
            SystemInfo = source.SystemInfo is null
                ? null
                : new Dictionary<string, QlogValue>(source.SystemInfo, StringComparer.Ordinal),
        };

        CopyEntries(source.Data, clone.Data);
        CopyEntries(source.ExtensionData, clone.ExtensionData);
        return clone;
    }

    private static QlogCommonFields? CloneCommonFields(QlogCommonFields? source)
    {
        if (source is null)
        {
            return null;
        }

        QlogCommonFields clone = new()
        {
            Tuple = source.Tuple,
            TimeFormat = source.TimeFormat,
            GroupId = source.GroupId,
            ReferenceTime = CloneReferenceTime(source.ReferenceTime),
        };

        CopyEntries(source.ExtensionData, clone.ExtensionData);
        return clone;
    }

    private static QlogReferenceTime? CloneReferenceTime(QlogReferenceTime? source)
    {
        if (source is null)
        {
            return null;
        }

        QlogReferenceTime clone = new()
        {
            ClockType = source.ClockType,
            Epoch = source.Epoch,
            WallClockTime = source.WallClockTime,
        };

        CopyEntries(source.ExtensionData, clone.ExtensionData);
        return clone;
    }

    private static QlogVantagePoint? CloneVantagePoint(QlogVantagePoint? source)
    {
        if (source is null)
        {
            return null;
        }

        QlogVantagePoint clone = new()
        {
            Name = source.Name,
            Type = source.Type,
            Flow = source.Flow,
        };

        CopyEntries(source.ExtensionData, clone.ExtensionData);
        return clone;
    }

    private static void CopyEntries(
        IEnumerable<KeyValuePair<string, QlogValue>> source,
        IDictionary<string, QlogValue> destination)
    {
        foreach (KeyValuePair<string, QlogValue> entry in source)
        {
            destination[entry.Key] = entry.Value;
        }
    }
}
