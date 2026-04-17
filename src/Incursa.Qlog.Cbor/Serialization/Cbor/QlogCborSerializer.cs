using System.Formats.Cbor;
using Incursa.Qlog.Serialization;
using JsonValueKind = System.Text.Json.JsonValueKind;

namespace Incursa.Qlog.Serialization.Cbor;

/// <summary>
/// Serializes contained qlog artifacts to CBOR.
/// </summary>
/// <remarks>
/// The first CBOR slice is contained-file only. The qlog file model must retain the contained file schema URI and
/// use <see cref="QlogCborKnownValues.ContainedCborSerializationFormat"/> as the <c>serialization_format</c>.
/// </remarks>
public static class QlogCborSerializer
{
    private const int FileMandatoryEntryCount = 3;
    private const int TraceMandatoryEntryCount = 2;
    private const int ReferenceTimeMandatoryEntryCount = 2;
    private const int EventMandatoryEntryCount = 3;

    /// <summary>
    /// Serializes a contained qlog file to a CBOR payload.
    /// </summary>
    /// <param name="file">The file to serialize.</param>
    /// <returns>The encoded CBOR payload.</returns>
    public static byte[] Serialize(QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using MemoryStream stream = new();
        Serialize(stream, file);
        return stream.ToArray();
    }

    /// <summary>
    /// Serializes a contained qlog file to a CBOR stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="file">The file to serialize.</param>
    public static void Serialize(Stream stream, QlogFile file)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(file);

        QlogSerializationCore.ValidateContainedFile(
            file,
            QlogCborKnownValues.ContainedCborSerializationFormat,
            "contained CBOR serializer");

        CborWriter writer = new(
            conformanceMode: CborConformanceMode.Strict,
            convertIndefiniteLengthEncodings: false,
            allowMultipleRootLevelValues: false);

        WriteContainedFile(writer, file);
        byte[] encoded = writer.Encode();
        stream.Write(encoded);
    }

    private static void WriteContainedFile(CborWriter writer, QlogFile file)
    {
        writer.WriteStartMap(GetFileEntryCount(file));
        writer.WriteTextString("file_schema");
        writer.WriteTextString(file.FileSchema.OriginalString);
        writer.WriteTextString("serialization_format");
        writer.WriteTextString(file.SerializationFormat);
        WriteOptionalTextString(writer, "title", file.Title);
        WriteOptionalTextString(writer, "description", file.Description);
        WriteMapEntries(writer, file.ExtensionData);
        writer.WriteTextString("traces");
        writer.WriteStartArray(file.Traces.Count);

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
                        $"Unsupported trace component type '{traceComponent?.GetType().FullName}'.");
            }
        }

        writer.WriteEndArray();
        writer.WriteEndMap();
    }

    private static void WriteTrace(CborWriter writer, QlogTrace trace)
    {
        writer.WriteStartMap(GetTraceEntryCount(trace));
        WriteOptionalTextString(writer, "title", trace.Title);
        WriteOptionalTextString(writer, "description", trace.Description);

        if (trace.CommonFields is not null)
        {
            writer.WriteTextString("common_fields");
            WriteCommonFields(writer, trace.CommonFields);
        }

        if (trace.VantagePoint is not null)
        {
            writer.WriteTextString("vantage_point");
            WriteVantagePoint(writer, trace.VantagePoint);
        }

        writer.WriteTextString("event_schemas");
        writer.WriteStartArray(trace.EventSchemas.Count);
        foreach (Uri eventSchema in trace.EventSchemas)
        {
            writer.WriteTextString(eventSchema.OriginalString);
        }

        writer.WriteEndArray();
        writer.WriteTextString("events");
        writer.WriteStartArray(trace.Events.Count);
        foreach (QlogEvent qlogEvent in trace.Events)
        {
            WriteEvent(writer, qlogEvent);
        }

        writer.WriteEndArray();
        WriteMapEntries(writer, trace.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteTraceError(CborWriter writer, QlogTraceError traceError)
    {
        writer.WriteStartMap(GetTraceErrorEntryCount(traceError));
        writer.WriteTextString("error_description");
        writer.WriteTextString(traceError.ErrorDescription);
        WriteOptionalTextString(writer, "uri", traceError.Uri);

        if (traceError.VantagePoint is not null)
        {
            writer.WriteTextString("vantage_point");
            WriteVantagePoint(writer, traceError.VantagePoint);
        }

        WriteMapEntries(writer, traceError.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteCommonFields(CborWriter writer, QlogCommonFields commonFields)
    {
        writer.WriteStartMap(GetCommonFieldsEntryCount(commonFields));
        WriteOptionalTextString(writer, "tuple", commonFields.Tuple);
        WriteOptionalTextString(writer, "time_format", commonFields.TimeFormat);
        WriteOptionalTextString(writer, "group_id", commonFields.GroupId);

        if (commonFields.ReferenceTime is not null)
        {
            writer.WriteTextString("reference_time");
            WriteReferenceTime(writer, commonFields.ReferenceTime);
        }

        WriteMapEntries(writer, commonFields.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteReferenceTime(CborWriter writer, QlogReferenceTime referenceTime)
    {
        writer.WriteStartMap(GetReferenceTimeEntryCount(referenceTime));
        writer.WriteTextString("clock_type");
        writer.WriteTextString(referenceTime.ClockType);
        writer.WriteTextString("epoch");
        writer.WriteTextString(referenceTime.Epoch);
        WriteOptionalTextString(writer, "wall_clock_time", referenceTime.WallClockTime);
        WriteMapEntries(writer, referenceTime.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteVantagePoint(CborWriter writer, QlogVantagePoint vantagePoint)
    {
        writer.WriteStartMap(GetVantagePointEntryCount(vantagePoint));
        WriteOptionalTextString(writer, "name", vantagePoint.Name);
        writer.WriteTextString("type");
        writer.WriteTextString(vantagePoint.Type);
        WriteOptionalTextString(writer, "flow", vantagePoint.Flow);
        WriteMapEntries(writer, vantagePoint.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteEvent(CborWriter writer, QlogEvent qlogEvent)
    {
        writer.WriteStartMap(GetEventEntryCount(qlogEvent));
        writer.WriteTextString("time");
        writer.WriteDouble(qlogEvent.Time);
        writer.WriteTextString("name");
        writer.WriteTextString(qlogEvent.Name);
        writer.WriteTextString("data");
        WriteValueMap(writer, qlogEvent.Data);
        WriteOptionalTextString(writer, "tuple", qlogEvent.Tuple);
        WriteOptionalTextString(writer, "time_format", qlogEvent.TimeFormat);
        WriteOptionalTextString(writer, "group_id", qlogEvent.GroupId);

        if (qlogEvent.SystemInfo is not null)
        {
            writer.WriteTextString("system_info");
            WriteValueMap(writer, qlogEvent.SystemInfo);
        }

        WriteMapEntries(writer, qlogEvent.ExtensionData);
        writer.WriteEndMap();
    }

    private static void WriteValueMap(CborWriter writer, IDictionary<string, QlogValue> values)
    {
        writer.WriteStartMap(values.Count);
        WriteMapEntries(writer, values);
        writer.WriteEndMap();
    }

    private static void WriteMapEntries(CborWriter writer, IEnumerable<KeyValuePair<string, QlogValue>> values)
    {
        foreach (KeyValuePair<string, QlogValue> entry in values)
        {
            writer.WriteTextString(entry.Key);
            WriteValue(writer, entry.Value);
        }
    }

    private static void WriteValue(CborWriter writer, QlogValue value)
    {
        if (!value.HasJsonValue)
        {
            writer.WriteNull();
            return;
        }

        WriteJsonElement(writer, value.JsonElement);
    }

    private static void WriteJsonElement(CborWriter writer, System.Text.Json.JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    System.Text.Json.JsonElement.ObjectEnumerator properties = element.EnumerateObject();
                    List<System.Text.Json.JsonProperty> propertyList = new();
                    foreach (System.Text.Json.JsonProperty property in properties)
                    {
                        propertyList.Add(property);
                    }

                    writer.WriteStartMap(propertyList.Count);
                    foreach (System.Text.Json.JsonProperty property in propertyList)
                    {
                        writer.WriteTextString(property.Name);
                        WriteJsonElement(writer, property.Value);
                    }

                    writer.WriteEndMap();
                    break;
                }

            case JsonValueKind.Array:
                {
                    System.Text.Json.JsonElement.ArrayEnumerator items = element.EnumerateArray();
                    List<System.Text.Json.JsonElement> itemList = new();
                    foreach (System.Text.Json.JsonElement item in items)
                    {
                        itemList.Add(item);
                    }

                    writer.WriteStartArray(itemList.Count);
                    foreach (System.Text.Json.JsonElement item in itemList)
                    {
                        WriteJsonElement(writer, item);
                    }

                    writer.WriteEndArray();
                    break;
                }

            case JsonValueKind.String:
                writer.WriteTextString(element.GetString() ?? string.Empty);
                break;

            case JsonValueKind.Number:
                if (element.TryGetInt64(out long signed))
                {
                    writer.WriteInt64(signed);
                }
                else if (element.TryGetUInt64(out ulong unsigned))
                {
                    writer.WriteUInt64(unsigned);
                }
                else
                {
                    writer.WriteDouble(element.GetDouble());
                }

                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.WriteBoolean(element.GetBoolean());
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                writer.WriteNull();
                break;

            default:
                throw new InvalidOperationException($"Unsupported JSON value kind '{element.ValueKind}'.");
        }
    }

    private static void WriteOptionalTextString(CborWriter writer, string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteTextString(propertyName);
            writer.WriteTextString(value);
        }
    }

    private static int GetFileEntryCount(QlogFile file) =>
        FileMandatoryEntryCount
        + GetOptionalEntryCount(file.Title)
        + GetOptionalEntryCount(file.Description)
        + file.ExtensionData.Count;

    private static int GetTraceEntryCount(QlogTrace trace) =>
        TraceMandatoryEntryCount
        + GetOptionalEntryCount(trace.Title)
        + GetOptionalEntryCount(trace.Description)
        + (trace.CommonFields is null ? 0 : 1)
        + (trace.VantagePoint is null ? 0 : 1)
        + trace.ExtensionData.Count;

    private static int GetTraceErrorEntryCount(QlogTraceError traceError) =>
        1
        + GetOptionalEntryCount(traceError.Uri)
        + (traceError.VantagePoint is null ? 0 : 1)
        + traceError.ExtensionData.Count;

    private static int GetCommonFieldsEntryCount(QlogCommonFields commonFields) =>
        GetOptionalEntryCount(commonFields.Tuple)
        + GetOptionalEntryCount(commonFields.TimeFormat)
        + GetOptionalEntryCount(commonFields.GroupId)
        + (commonFields.ReferenceTime is null ? 0 : 1)
        + commonFields.ExtensionData.Count;

    private static int GetReferenceTimeEntryCount(QlogReferenceTime referenceTime) =>
        ReferenceTimeMandatoryEntryCount
        + GetOptionalEntryCount(referenceTime.WallClockTime)
        + referenceTime.ExtensionData.Count;

    private static int GetVantagePointEntryCount(QlogVantagePoint vantagePoint) =>
        1
        + GetOptionalEntryCount(vantagePoint.Name)
        + GetOptionalEntryCount(vantagePoint.Flow)
        + vantagePoint.ExtensionData.Count;

    private static int GetEventEntryCount(QlogEvent qlogEvent) =>
        EventMandatoryEntryCount
        + GetOptionalEntryCount(qlogEvent.Tuple)
        + GetOptionalEntryCount(qlogEvent.TimeFormat)
        + GetOptionalEntryCount(qlogEvent.GroupId)
        + (qlogEvent.SystemInfo is null ? 0 : 1)
        + qlogEvent.ExtensionData.Count;

    private static int GetOptionalEntryCount(string? value) => string.IsNullOrWhiteSpace(value) ? 0 : 1;
}
