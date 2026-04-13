using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Incursa.Qlog.Serialization.Json;

/// <summary>
/// Serializes and parses contained qlog JSON artifacts for the v1 core baseline.
/// </summary>
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

        ValidateContainedFile(file);

        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions
        {
            Indented = indented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        WriteFile(writer, file);
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

    private static void ValidateContainedFile(QlogFile file)
    {
        if (file.FileSchema is null || !file.FileSchema.IsAbsoluteUri)
        {
            throw new InvalidOperationException("Contained qlog files require an absolute file_schema URI.");
        }

        if (!Uri.Equals(file.FileSchema, QlogKnownValues.ContainedFileSchemaUri))
        {
            throw new InvalidOperationException(
                $"The contained JSON serializer only supports '{QlogKnownValues.ContainedFileSchemaUri}'.");
        }

        if (string.IsNullOrWhiteSpace(file.SerializationFormat))
        {
            throw new InvalidOperationException("Contained qlog files require a serialization_format media type.");
        }

        if (!string.Equals(
            file.SerializationFormat,
            QlogKnownValues.ContainedJsonSerializationFormat,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The contained JSON serializer only supports '{QlogKnownValues.ContainedJsonSerializationFormat}'.");
        }

        if (file.Traces.Count == 0)
        {
            throw new InvalidOperationException(
                "Contained qlog files must carry at least one component trace or explicit trace error.");
        }

        foreach (QlogTraceComponent traceComponent in file.Traces)
        {
            switch (traceComponent)
            {
                case QlogTrace trace:
                    ValidateTrace(trace);
                    break;
                case QlogTraceError traceError:
                    ValidateTraceError(traceError);
                    break;
                case null:
                    throw new InvalidOperationException("Contained qlog files cannot include null trace entries.");
                default:
                    throw new InvalidOperationException(
                        $"Unsupported trace component type '{traceComponent.GetType().FullName}'.");
            }
        }
    }

    private static void ValidateTrace(QlogTrace trace)
    {
        ArgumentNullException.ThrowIfNull(trace);

        if (trace.EventSchemas.Count == 0)
        {
            throw new InvalidOperationException("A qlog trace must declare at least one event schema URI.");
        }

        foreach (Uri eventSchema in trace.EventSchemas)
        {
            if (eventSchema is null || !eventSchema.IsAbsoluteUri)
            {
                throw new InvalidOperationException("A qlog event schema URI must be absolute.");
            }
        }

        foreach (QlogEvent qlogEvent in trace.Events)
        {
            ValidateEvent(qlogEvent);
        }
    }

    private static void ValidateTraceError(QlogTraceError traceError)
    {
        ArgumentNullException.ThrowIfNull(traceError);

        if (string.IsNullOrWhiteSpace(traceError.ErrorDescription))
        {
            throw new InvalidOperationException("A qlog trace error requires an error_description value.");
        }
    }

    private static void ValidateEvent(QlogEvent qlogEvent)
    {
        ArgumentNullException.ThrowIfNull(qlogEvent);

        if (double.IsNaN(qlogEvent.Time) || double.IsInfinity(qlogEvent.Time))
        {
            throw new InvalidOperationException("A qlog event time must be a finite number.");
        }

        if (string.IsNullOrWhiteSpace(qlogEvent.Name))
        {
            throw new InvalidOperationException("A qlog event requires a namespace-qualified name.");
        }

        int separatorIndex = qlogEvent.Name.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex <= 0 || separatorIndex == qlogEvent.Name.Length - 1)
        {
            throw new InvalidOperationException("A qlog event name must combine a namespace, ':', and an event type.");
        }
    }

    private static void WriteFile(Utf8JsonWriter writer, QlogFile file)
    {
        writer.WriteStartObject();
        writer.WriteString("file_schema", file.FileSchema.OriginalString);
        writer.WriteString("serialization_format", file.SerializationFormat);
        WriteOptionalString(writer, "title", file.Title);
        WriteOptionalString(writer, "description", file.Description);
        WriteExtensionData(writer, file.ExtensionData);

        writer.WritePropertyName("traces");
        writer.WriteStartArray();

        foreach (QlogTraceComponent traceComponent in file.Traces)
        {
            switch (traceComponent)
            {
                case QlogTrace trace:
                    WriteTrace(writer, trace);
                    break;
                case QlogTraceError traceError:
                    WriteTraceError(writer, traceError);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported trace component type '{traceComponent.GetType().FullName}'.");
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteTrace(Utf8JsonWriter writer, QlogTrace trace)
    {
        writer.WriteStartObject();
        WriteOptionalString(writer, "title", trace.Title);
        WriteOptionalString(writer, "description", trace.Description);

        if (trace.CommonFields is not null)
        {
            writer.WritePropertyName("common_fields");
            WriteCommonFields(writer, trace.CommonFields);
        }

        if (trace.VantagePoint is not null)
        {
            writer.WritePropertyName("vantage_point");
            WriteVantagePoint(writer, trace.VantagePoint);
        }

        writer.WritePropertyName("event_schemas");
        writer.WriteStartArray();
        foreach (Uri eventSchema in trace.EventSchemas)
        {
            writer.WriteStringValue(eventSchema.OriginalString);
        }

        writer.WriteEndArray();

        writer.WritePropertyName("events");
        writer.WriteStartArray();
        foreach (QlogEvent qlogEvent in trace.Events)
        {
            WriteEvent(writer, qlogEvent);
        }

        writer.WriteEndArray();
        WriteExtensionData(writer, trace.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteTraceError(Utf8JsonWriter writer, QlogTraceError traceError)
    {
        writer.WriteStartObject();
        writer.WriteString("error_description", traceError.ErrorDescription);
        WriteOptionalString(writer, "uri", traceError.Uri);

        if (traceError.VantagePoint is not null)
        {
            writer.WritePropertyName("vantage_point");
            WriteVantagePoint(writer, traceError.VantagePoint);
        }

        WriteExtensionData(writer, traceError.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteCommonFields(Utf8JsonWriter writer, QlogCommonFields commonFields)
    {
        writer.WriteStartObject();
        WriteOptionalString(writer, "tuple", commonFields.Tuple);
        WriteOptionalString(writer, "time_format", commonFields.TimeFormat);
        WriteOptionalString(writer, "group_id", commonFields.GroupId);

        if (commonFields.ReferenceTime is not null)
        {
            writer.WritePropertyName("reference_time");
            WriteReferenceTime(writer, commonFields.ReferenceTime);
        }

        WriteExtensionData(writer, commonFields.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteReferenceTime(Utf8JsonWriter writer, QlogReferenceTime referenceTime)
    {
        writer.WriteStartObject();
        writer.WriteString("clock_type", referenceTime.ClockType);
        writer.WriteString("epoch", referenceTime.Epoch);
        WriteOptionalString(writer, "wall_clock_time", referenceTime.WallClockTime);
        WriteExtensionData(writer, referenceTime.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteVantagePoint(Utf8JsonWriter writer, QlogVantagePoint vantagePoint)
    {
        writer.WriteStartObject();
        WriteOptionalString(writer, "name", vantagePoint.Name);
        writer.WriteString("type", vantagePoint.Type);
        WriteOptionalString(writer, "flow", vantagePoint.Flow);
        WriteExtensionData(writer, vantagePoint.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteEvent(Utf8JsonWriter writer, QlogEvent qlogEvent)
    {
        writer.WriteStartObject();
        writer.WriteNumber("time", qlogEvent.Time);
        writer.WriteString("name", qlogEvent.Name);

        writer.WritePropertyName("data");
        writer.WriteStartObject();
        WriteExtensionData(writer, qlogEvent.Data);
        writer.WriteEndObject();

        WriteOptionalString(writer, "tuple", qlogEvent.Tuple);
        WriteOptionalString(writer, "time_format", qlogEvent.TimeFormat);
        WriteOptionalString(writer, "group_id", qlogEvent.GroupId);

        if (qlogEvent.SystemInfo is not null)
        {
            writer.WritePropertyName("system_info");
            writer.WriteStartObject();
            WriteExtensionData(writer, qlogEvent.SystemInfo);
            writer.WriteEndObject();
        }

        WriteExtensionData(writer, qlogEvent.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteOptionalString(Utf8JsonWriter writer, string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteString(propertyName, value);
        }
    }

    private static void WriteExtensionData(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, QlogValue>> extensionData)
    {
        foreach (KeyValuePair<string, QlogValue> entry in extensionData)
        {
            writer.WritePropertyName(entry.Key);
            entry.Value.WriteTo(writer);
        }
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

        ValidateContainedFile(file);
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
