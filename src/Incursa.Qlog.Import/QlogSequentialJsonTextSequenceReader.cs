using System.Text.Json;
using Incursa.Qlog.Serialization;

namespace Incursa.Qlog.Import;

internal static class QlogSequentialJsonTextSequenceReader
{
    private const char RecordSeparator = '\u001E';

    public static QlogFile Deserialize(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string trimmedText = TrimOuterWhitespaceAndBom(text);
        if (trimmedText.Length == 0)
        {
            throw new InvalidOperationException("Sequential qlog input must not be empty.");
        }

        if (trimmedText[0] != RecordSeparator)
        {
            throw new InvalidOperationException("Sequential qlog input must start with an RS-delimited record.");
        }

        QlogFile file = new();
        QlogTrace? trace = null;
        int recordIndex = 0;
        int cursor = 0;

        while (cursor < trimmedText.Length)
        {
            if (trimmedText[cursor] != RecordSeparator)
            {
                throw new InvalidOperationException("Sequential qlog input must use RS-delimited records.");
            }

            int recordStart = cursor + 1;
            int nextRecordSeparator = trimmedText.IndexOf(RecordSeparator, recordStart);
            int recordEnd = nextRecordSeparator >= 0 ? nextRecordSeparator : trimmedText.Length;
            string recordText = trimmedText.Substring(recordStart, recordEnd - recordStart).Trim();

            if (recordText.Length == 0)
            {
                throw new InvalidOperationException("Sequential qlog input contains an empty record.");
            }

            using JsonDocument document = JsonDocument.Parse(recordText);
            if (recordIndex == 0)
            {
                trace = ReadHeaderRecord(document.RootElement, file);
                file.Traces.Add(trace);
            }
            else
            {
                if (trace is null)
                {
                    throw new InvalidOperationException("Sequential qlog input is missing a trace header record.");
                }

                trace.Events.Add(ReadEvent(document.RootElement));
            }

            recordIndex++;
            cursor = recordEnd;

            if (nextRecordSeparator < 0)
            {
                break;
            }
        }

        if (recordIndex == 0)
        {
            throw new InvalidOperationException("Sequential qlog input must contain a trace header record.");
        }

        QlogSerializationCore.ValidateSequentialFile(file);
        return file;
    }

    private static QlogTrace ReadHeaderRecord(JsonElement element, QlogFile file)
    {
        EnsureObject(element, "sequential qlog header");

        QlogTrace? trace = null;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "file_schema":
                    file.FileSchema = ReadAbsoluteUri(property.Value, "file_schema");
                    break;
                case "serialization_format":
                    file.SerializationFormat = property.Value.GetString() ?? string.Empty;
                    break;
                case "title":
                    file.Title = property.Value.GetString();
                    break;
                case "description":
                    file.Description = property.Value.GetString();
                    break;
                case "trace":
                    trace = ReadTraceHeader(property.Value);
                    break;
                default:
                    file.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        if (trace is null)
        {
            throw new InvalidOperationException("Sequential qlog input must include a trace header record.");
        }

        return trace;
    }

    private static QlogTrace ReadTraceHeader(JsonElement element)
    {
        EnsureObject(element, "trace header");

        QlogTrace trace = new();

        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "title":
                    trace.Title = property.Value.GetString();
                    break;
                case "description":
                    trace.Description = property.Value.GetString();
                    break;
                case "common_fields":
                    trace.CommonFields = ReadCommonFields(property.Value);
                    break;
                case "vantage_point":
                    trace.VantagePoint = ReadVantagePoint(property.Value);
                    break;
                case "event_schemas":
                    foreach (JsonElement uriElement in property.Value.EnumerateArray())
                    {
                        trace.EventSchemas.Add(ReadAbsoluteUri(uriElement, "event_schemas"));
                    }

                    break;
                case "events":
                    throw new InvalidOperationException("Sequential qlog trace headers cannot include events.");
                default:
                    trace.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        return trace;
    }

    private static QlogEvent ReadEvent(JsonElement element)
    {
        EnsureObject(element, "event");

        QlogEvent qlogEvent = new();
        bool sawTime = false;
        bool sawName = false;
        bool sawData = false;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "time":
                    qlogEvent.Time = property.Value.GetDouble();
                    sawTime = true;
                    break;
                case "name":
                    qlogEvent.Name = property.Value.GetString() ?? string.Empty;
                    sawName = true;
                    break;
                case "data":
                    CopyObjectMap(qlogEvent.Data, property.Value, "data");
                    sawData = true;
                    break;
                case "tuple":
                    qlogEvent.Tuple = property.Value.GetString();
                    break;
                case "time_format":
                    qlogEvent.TimeFormat = property.Value.GetString();
                    break;
                case "group_id":
                    qlogEvent.GroupId = property.Value.GetString();
                    break;
                case "system_info":
                    qlogEvent.SystemInfo = ReadObjectMap(property.Value, "system_info");
                    break;
                default:
                    qlogEvent.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        if (!sawTime)
        {
            throw new InvalidOperationException("Sequential qlog event records must include a time value.");
        }

        if (!sawName)
        {
            throw new InvalidOperationException("Sequential qlog event records must include a name value.");
        }

        if (!sawData)
        {
            throw new InvalidOperationException("Sequential qlog event records must include a data object.");
        }

        return qlogEvent;
    }

    private static QlogCommonFields ReadCommonFields(JsonElement element)
    {
        EnsureObject(element, "common_fields");

        QlogCommonFields commonFields = new();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "tuple":
                    commonFields.Tuple = property.Value.GetString();
                    break;
                case "time_format":
                    commonFields.TimeFormat = property.Value.GetString();
                    break;
                case "reference_time":
                    commonFields.ReferenceTime = ReadReferenceTime(property.Value);
                    break;
                case "group_id":
                    commonFields.GroupId = property.Value.GetString();
                    break;
                default:
                    commonFields.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        return commonFields;
    }

    private static QlogReferenceTime ReadReferenceTime(JsonElement element)
    {
        EnsureObject(element, "reference_time");

        QlogReferenceTime referenceTime = new();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "clock_type":
                    referenceTime.ClockType = property.Value.GetString() ?? referenceTime.ClockType;
                    break;
                case "epoch":
                    referenceTime.Epoch = property.Value.GetString() ?? referenceTime.Epoch;
                    break;
                case "wall_clock_time":
                    referenceTime.WallClockTime = property.Value.GetString();
                    break;
                default:
                    referenceTime.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        return referenceTime;
    }

    private static QlogVantagePoint ReadVantagePoint(JsonElement element)
    {
        EnsureObject(element, "vantage_point");

        QlogVantagePoint vantagePoint = new();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "name":
                    vantagePoint.Name = property.Value.GetString();
                    break;
                case "type":
                    vantagePoint.Type = property.Value.GetString() ?? vantagePoint.Type;
                    break;
                case "flow":
                    vantagePoint.Flow = property.Value.GetString();
                    break;
                default:
                    vantagePoint.ExtensionData[property.Name] = ParseValue(property.Value);
                    break;
            }
        }

        return vantagePoint;
    }

    private static Dictionary<string, QlogValue> ReadObjectMap(JsonElement element, string description)
    {
        EnsureObject(element, description);

        Dictionary<string, QlogValue> map = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            map[property.Name] = ParseValue(property.Value);
        }

        return map;
    }

    private static void CopyObjectMap(IDictionary<string, QlogValue> target, JsonElement element, string description)
    {
        target.Clear();
        foreach (KeyValuePair<string, QlogValue> entry in ReadObjectMap(element, description))
        {
            target[entry.Key] = entry.Value;
        }
    }

    private static Uri ReadAbsoluteUri(JsonElement element, string description)
    {
        string? value = element.GetString();
        if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            throw new InvalidOperationException($"Sequential qlog {description} must be an absolute URI.");
        }

        return uri;
    }

    private static QlogValue ParseValue(JsonElement element)
    {
        return QlogValue.Parse(element.GetRawText());
    }

    private static void EnsureObject(JsonElement element, string description)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException($"Sequential qlog {description} must be a JSON object.");
        }
    }

    private static string TrimOuterWhitespaceAndBom(string text)
    {
        int start = 0;
        while (start < text.Length && (char.IsWhiteSpace(text[start]) || text[start] == '\uFEFF'))
        {
            start++;
        }

        int end = text.Length - 1;
        while (end >= start && char.IsWhiteSpace(text[end]))
        {
            end--;
        }

        return end < start ? string.Empty : text.Substring(start, end - start + 1);
    }
}
