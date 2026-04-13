using System.Text.Json;

namespace Incursa.Qlog.Serialization;

/// <summary>
/// Provides shared qlog envelope validation and model traversal for format-specific serializers.
/// </summary>
internal static class QlogSerializationCore
{
    /// <summary>
    /// Validates a contained qlog file envelope for serialization.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    public static void ValidateContainedFile(QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        ValidateFileEnvelope(
            file,
            QlogKnownValues.ContainedFileSchemaUri,
            QlogKnownValues.ContainedJsonSerializationFormat,
            "contained JSON serializer");

        if (file.Traces.Count == 0)
        {
            throw new InvalidOperationException(
                "Contained qlog files must carry at least one component trace or explicit trace error.");
        }

        foreach (QlogTraceComponent traceComponent in file.Traces)
        {
            ValidateContainedTraceComponent(traceComponent);
        }
    }

    /// <summary>
    /// Validates a sequential qlog file envelope for serialization.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    public static void ValidateSequentialFile(QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        ValidateFileEnvelope(
            file,
            QlogKnownValues.SequentialFileSchemaUri,
            QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
            "sequential JSON Text Sequences serializer");

        if (file.Traces.Count != 1)
        {
            throw new InvalidOperationException("Sequential qlog files must carry exactly one trace component.");
        }

        if (file.Traces[0] is not QlogTrace trace)
        {
            throw new InvalidOperationException("Sequential qlog files must carry a trace component.");
        }

        ValidateTrace(trace);
    }

    /// <summary>
    /// Writes a contained qlog file to a JSON writer.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="file">The file to serialize.</param>
    public static void WriteContainedFile(Utf8JsonWriter writer, QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        writer.WriteStartObject();
        WriteFileEnvelope(writer, file);

        writer.WritePropertyName("traces");
        writer.WriteStartArray();

        foreach (QlogTraceComponent traceComponent in file.Traces)
        {
            switch (traceComponent)
            {
                case QlogTrace trace:
                    WriteContainedTrace(writer, trace);
                    break;
                case QlogTraceError traceError:
                    WriteTraceError(writer, traceError);
                    break;
                case null:
                    throw new InvalidOperationException("Contained qlog files cannot include null trace entries.");
                default:
                    throw new InvalidOperationException(
                        $"Unsupported trace component type '{traceComponent.GetType().FullName}'.");
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a sequential qlog file header record to a JSON writer.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="file">The file to serialize.</param>
    public static void WriteSequentialFileHeader(Utf8JsonWriter writer, QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        if (file.Traces.Count != 1 || file.Traces[0] is not QlogTrace trace)
        {
            throw new InvalidOperationException("Sequential qlog files must carry a trace component.");
        }

        writer.WriteStartObject();
        WriteFileEnvelope(writer, file);
        writer.WritePropertyName("trace");
        WriteTraceHeader(writer, trace);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a qlog event object to a JSON writer.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="qlogEvent">The event to serialize.</param>
    public static void WriteEvent(Utf8JsonWriter writer, QlogEvent qlogEvent)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(qlogEvent);

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

    private static void ValidateFileEnvelope(
        QlogFile file,
        Uri expectedFileSchema,
        string expectedSerializationFormat,
        string serializerLabel)
    {
        if (file.FileSchema is null || !file.FileSchema.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{serializerLabel} requires an absolute file_schema URI.");
        }

        if (!Uri.Equals(file.FileSchema, expectedFileSchema))
        {
            throw new InvalidOperationException(
                $"The {serializerLabel} only supports '{expectedFileSchema}'.");
        }

        if (string.IsNullOrWhiteSpace(file.SerializationFormat))
        {
            throw new InvalidOperationException($"{serializerLabel} requires a serialization_format media type.");
        }

        if (!string.Equals(
            file.SerializationFormat,
            expectedSerializationFormat,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The {serializerLabel} only supports '{expectedSerializationFormat}'.");
        }
    }

    private static void ValidateContainedTraceComponent(QlogTraceComponent? traceComponent)
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

    private static void WriteFileEnvelope(Utf8JsonWriter writer, QlogFile file)
    {
        writer.WriteString("file_schema", file.FileSchema.OriginalString);
        writer.WriteString("serialization_format", file.SerializationFormat);
        WriteOptionalString(writer, "title", file.Title);
        WriteOptionalString(writer, "description", file.Description);
        WriteExtensionData(writer, file.ExtensionData);
    }

    private static void WriteContainedTrace(Utf8JsonWriter writer, QlogTrace trace)
    {
        writer.WriteStartObject();
        WriteTraceFields(writer, trace);

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

    private static void WriteTraceHeader(Utf8JsonWriter writer, QlogTrace trace)
    {
        writer.WriteStartObject();
        WriteTraceFields(writer, trace);
        WriteExtensionData(writer, trace.ExtensionData);
        writer.WriteEndObject();
    }

    private static void WriteTraceFields(Utf8JsonWriter writer, QlogTrace trace)
    {
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
}
