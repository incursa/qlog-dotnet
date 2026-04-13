using System.Text;
using System.Text.Json;
using Incursa.Qlog.Serialization;

namespace Incursa.Qlog.Serialization.Json;

/// <summary>
/// Serializes and parses contained qlog JSON artifacts for the v1 core baseline.
/// </summary>
/// <remarks>
/// Use this serializer for qlog envelopes that carry a <c>traces</c> array. For sequential JSON Text Sequences,
/// use <see cref="QlogJsonTextSequenceSerializer"/>.
/// </remarks>
public static class QlogJsonSerializer
{
    /// <summary>
    /// Serializes a contained qlog file to a JSON string.
    /// </summary>
    /// <param name="file">The file to serialize.</param>
    /// <param name="indented">Indicates whether the output should be indented.</param>
    /// <returns>The serialized JSON text.</returns>
    public static string Serialize(QlogFile file, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(file);

        using MemoryStream stream = new();
        Serialize(stream, file, indented);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Serializes a contained qlog file to a UTF-8 JSON stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="file">The file to serialize.</param>
    /// <param name="indented">Indicates whether the output should be indented.</param>
    public static void Serialize(Stream stream, QlogFile file, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(file);

        QlogSerializationCore.ValidateContainedFile(file);

        using Utf8JsonWriter writer = new(stream, QlogJsonSerializationHelpers.CreateWriterOptions(indented));
        QlogSerializationCore.WriteContainedFile(writer, file);
        writer.Flush();
    }

    /// <summary>
    /// Parses a contained qlog file from a JSON string.
    /// </summary>
    /// <param name="json">The contained qlog JSON text.</param>
    /// <returns>The parsed file model.</returns>
    public static QlogFile Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument document = JsonDocument.Parse(json);
        return ReadFile(document.RootElement);
    }

    /// <summary>
    /// Parses a contained qlog file from a JSON stream.
    /// </summary>
    /// <param name="stream">The UTF-8 JSON stream.</param>
    /// <returns>The parsed file model.</returns>
    public static QlogFile Deserialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using JsonDocument document = JsonDocument.Parse(stream);
        return ReadFile(document.RootElement);
    }

    private static QlogFile ReadFile(JsonElement element)
    {
        EnsureObject(element, "qlog file");

        QlogFile file = new();

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
                case "traces":
                    foreach (JsonElement traceElement in property.Value.EnumerateArray())
                    {
                        file.Traces.Add(ReadTraceComponent(traceElement));
                    }

                    break;
                default:
                    file.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
                    break;
            }
        }

        QlogSerializationCore.ValidateContainedFile(file);
        return file;
    }

    private static QlogTraceComponent ReadTraceComponent(JsonElement element)
    {
        EnsureObject(element, "trace component");

        return element.TryGetProperty("error_description", out _)
            ? ReadTraceError(element)
            : ReadTrace(element);
    }

    private static QlogTrace ReadTrace(JsonElement element)
    {
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
                    foreach (JsonElement eventElement in property.Value.EnumerateArray())
                    {
                        trace.Events.Add(ReadEvent(eventElement));
                    }

                    break;
                default:
                    trace.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
                    break;
            }
        }

        return trace;
    }

    private static QlogTraceError ReadTraceError(JsonElement element)
    {
        QlogTraceError traceError = new();

        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "error_description":
                    traceError.ErrorDescription = property.Value.GetString() ?? string.Empty;
                    break;
                case "uri":
                    traceError.Uri = property.Value.GetString();
                    break;
                case "vantage_point":
                    traceError.VantagePoint = ReadVantagePoint(property.Value);
                    break;
                default:
                    traceError.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
                    break;
            }
        }

        return traceError;
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
                    commonFields.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
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
                    referenceTime.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
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
                    vantagePoint.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
                    break;
            }
        }

        return vantagePoint;
    }

    private static QlogEvent ReadEvent(JsonElement element)
    {
        EnsureObject(element, "event");

        QlogEvent qlogEvent = new();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case "time":
                    qlogEvent.Time = property.Value.GetDouble();
                    break;
                case "name":
                    qlogEvent.Name = property.Value.GetString() ?? string.Empty;
                    break;
                case "data":
                    qlogEvent.Data.Clear();
                    foreach (JsonProperty dataProperty in property.Value.EnumerateObject())
                    {
                        qlogEvent.Data[dataProperty.Name] = QlogValue.FromElement(dataProperty.Value);
                    }

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
                    qlogEvent.ExtensionData[property.Name] = QlogValue.FromElement(property.Value);
                    break;
            }
        }

        return qlogEvent;
    }

    private static Dictionary<string, QlogValue> ReadObjectMap(JsonElement element, string description)
    {
        EnsureObject(element, description);

        Dictionary<string, QlogValue> map = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            map[property.Name] = QlogValue.FromElement(property.Value);
        }

        return map;
    }

    private static Uri ReadAbsoluteUri(JsonElement element, string propertyName)
    {
        string? uriText = element.GetString();
        if (string.IsNullOrWhiteSpace(uriText) || !Uri.TryCreate(uriText, UriKind.Absolute, out Uri? uri))
        {
            throw new FormatException($"Property '{propertyName}' must be an absolute URI.");
        }

        return uri;
    }

    private static void EnsureObject(JsonElement element, string description)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException($"{description} must be a JSON object.");
        }
    }
}
