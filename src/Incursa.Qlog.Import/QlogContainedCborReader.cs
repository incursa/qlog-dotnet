using System.Globalization;
using System.Formats.Cbor;
using System.Numerics;
using Incursa.Qlog.Serialization;

namespace Incursa.Qlog.Import;

internal static class QlogContainedCborReader
{
    private const string ContainedCborSerializationFormat = "application/cbor";

    public static QlogFile Deserialize(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (payload.Length == 0)
        {
            throw new InvalidOperationException("Contained qlog CBOR input must not be empty.");
        }

        try
        {
            CborReader reader = new(
                payload,
                CborConformanceMode.Strict,
                allowMultipleRootLevelValues: false);

            object? rootValue = ReadValue(reader);
            if (reader.BytesRemaining != 0)
            {
                throw new InvalidOperationException("Contained qlog CBOR input must contain exactly one root value.");
            }

            Dictionary<string, object?> root = RequireMap(rootValue, "qlog file");
            QlogFile file = ReadFile(root);
            QlogSerializationCore.ValidateContainedFile(file, ContainedCborSerializationFormat, "contained CBOR import");
            return file;
        }
        catch (CborContentException ex)
        {
            throw new InvalidOperationException("Malformed contained qlog CBOR input.", ex);
        }
    }

    private static QlogFile ReadFile(Dictionary<string, object?> root)
    {
        QlogFile file = new();
        bool sawFileSchema = false;
        bool sawSerializationFormat = false;

        foreach (KeyValuePair<string, object?> entry in root)
        {
            switch (entry.Key)
            {
                case "file_schema":
                    file.FileSchema = ReadAbsoluteUri(entry.Value, "file_schema");
                    sawFileSchema = true;
                    break;
                case "serialization_format":
                    file.SerializationFormat = ReadString(entry.Value, "serialization_format");
                    sawSerializationFormat = true;
                    break;
                case "title":
                    file.Title = ReadNullableString(entry.Value, "title");
                    break;
                case "description":
                    file.Description = ReadNullableString(entry.Value, "description");
                    break;
                case "traces":
                    foreach (object? traceComponentValue in RequireArray(entry.Value, "traces"))
                    {
                        file.Traces.Add(ReadTraceComponent(traceComponentValue));
                    }

                    break;
                default:
                    file.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        if (!sawFileSchema)
        {
            throw new InvalidOperationException("Contained qlog CBOR input must include file_schema.");
        }

        if (!sawSerializationFormat)
        {
            throw new InvalidOperationException("Contained qlog CBOR input must include serialization_format.");
        }

        return file;
    }

    private static QlogTraceComponent ReadTraceComponent(object? value)
    {
        Dictionary<string, object?> map = RequireMap(value, "trace component");
        bool hasErrorDescription = map.ContainsKey("error_description");
        bool hasEvents = map.ContainsKey("events");

        if (hasErrorDescription && hasEvents)
        {
            throw new InvalidOperationException("Contained qlog trace components cannot mix trace and trace-error shapes.");
        }

        if (hasErrorDescription)
        {
            return ReadTraceError(map);
        }

        if (hasEvents)
        {
            return ReadTrace(map);
        }

        throw new InvalidOperationException("Contained qlog trace components must be either traces or trace errors.");
    }

    private static QlogTrace ReadTrace(Dictionary<string, object?> map)
    {
        QlogTrace trace = new();
        bool sawEvents = false;

        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "title":
                    trace.Title = ReadNullableString(entry.Value, "title");
                    break;
                case "description":
                    trace.Description = ReadNullableString(entry.Value, "description");
                    break;
                case "common_fields":
                    trace.CommonFields = ReadCommonFields(entry.Value);
                    break;
                case "vantage_point":
                    trace.VantagePoint = ReadVantagePoint(entry.Value);
                    break;
                case "event_schemas":
                    foreach (object? schemaValue in RequireArray(entry.Value, "event_schemas"))
                    {
                        trace.EventSchemas.Add(ReadAbsoluteUri(schemaValue, "event_schemas"));
                    }

                    break;
                case "events":
                    foreach (object? eventValue in RequireArray(entry.Value, "events"))
                    {
                        trace.Events.Add(ReadEvent(eventValue));
                    }

                    sawEvents = true;
                    break;
                default:
                    trace.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        if (!sawEvents)
        {
            throw new InvalidOperationException("Contained qlog traces must include events.");
        }

        return trace;
    }

    private static QlogTraceError ReadTraceError(Dictionary<string, object?> map)
    {
        QlogTraceError traceError = new();
        bool sawErrorDescription = false;

        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "error_description":
                    traceError.ErrorDescription = ReadString(entry.Value, "error_description");
                    sawErrorDescription = true;
                    break;
                case "uri":
                    traceError.Uri = ReadNullableString(entry.Value, "uri");
                    break;
                case "vantage_point":
                    traceError.VantagePoint = ReadVantagePoint(entry.Value);
                    break;
                default:
                    traceError.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        if (!sawErrorDescription)
        {
            throw new InvalidOperationException("Contained qlog trace errors must include error_description.");
        }

        return traceError;
    }

    private static QlogCommonFields ReadCommonFields(object? value)
    {
        Dictionary<string, object?> map = RequireMap(value, "common_fields");

        QlogCommonFields commonFields = new();
        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "tuple":
                    commonFields.Tuple = ReadNullableString(entry.Value, "tuple");
                    break;
                case "time_format":
                    commonFields.TimeFormat = ReadNullableString(entry.Value, "time_format");
                    break;
                case "reference_time":
                    commonFields.ReferenceTime = ReadReferenceTime(entry.Value);
                    break;
                case "group_id":
                    commonFields.GroupId = ReadNullableString(entry.Value, "group_id");
                    break;
                default:
                    commonFields.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        return commonFields;
    }

    private static QlogReferenceTime ReadReferenceTime(object? value)
    {
        Dictionary<string, object?> map = RequireMap(value, "reference_time");

        QlogReferenceTime referenceTime = new();
        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "clock_type":
                    referenceTime.ClockType = ReadString(entry.Value, "clock_type");
                    break;
                case "epoch":
                    referenceTime.Epoch = ReadString(entry.Value, "epoch");
                    break;
                case "wall_clock_time":
                    referenceTime.WallClockTime = ReadNullableString(entry.Value, "wall_clock_time");
                    break;
                default:
                    referenceTime.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        return referenceTime;
    }

    private static QlogVantagePoint ReadVantagePoint(object? value)
    {
        Dictionary<string, object?> map = RequireMap(value, "vantage_point");

        QlogVantagePoint vantagePoint = new();
        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "name":
                    vantagePoint.Name = ReadNullableString(entry.Value, "name");
                    break;
                case "type":
                    vantagePoint.Type = ReadString(entry.Value, "type");
                    break;
                case "flow":
                    vantagePoint.Flow = ReadNullableString(entry.Value, "flow");
                    break;
                default:
                    vantagePoint.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        return vantagePoint;
    }

    private static QlogEvent ReadEvent(object? value)
    {
        Dictionary<string, object?> map = RequireMap(value, "event");

        QlogEvent qlogEvent = new();
        bool sawTime = false;
        bool sawName = false;
        bool sawData = false;

        foreach (KeyValuePair<string, object?> entry in map)
        {
            switch (entry.Key)
            {
                case "time":
                    qlogEvent.Time = ReadFiniteDouble(entry.Value, "time");
                    sawTime = true;
                    break;
                case "name":
                    qlogEvent.Name = ReadString(entry.Value, "name");
                    sawName = true;
                    break;
                case "data":
                    CopyObjectMap(qlogEvent.Data, entry.Value, "data");
                    sawData = true;
                    break;
                case "tuple":
                    qlogEvent.Tuple = ReadNullableString(entry.Value, "tuple");
                    break;
                case "time_format":
                    qlogEvent.TimeFormat = ReadNullableString(entry.Value, "time_format");
                    break;
                case "group_id":
                    qlogEvent.GroupId = ReadNullableString(entry.Value, "group_id");
                    break;
                case "system_info":
                    qlogEvent.SystemInfo = ReadObjectMap(entry.Value, "system_info");
                    break;
                default:
                    qlogEvent.ExtensionData[entry.Key] = ReadQlogValue(entry.Value);
                    break;
            }
        }

        if (!sawTime)
        {
            throw new InvalidOperationException("Contained qlog events must include time.");
        }

        if (!sawName)
        {
            throw new InvalidOperationException("Contained qlog events must include name.");
        }

        if (!sawData)
        {
            throw new InvalidOperationException("Contained qlog events must include data.");
        }

        return qlogEvent;
    }

    private static Dictionary<string, QlogValue> ReadObjectMap(object? value, string description)
    {
        Dictionary<string, object?> map = RequireMap(value, description);
        Dictionary<string, QlogValue> result = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, object?> entry in map)
        {
            result[entry.Key] = ReadQlogValue(entry.Value);
        }

        return result;
    }

    private static void CopyObjectMap(IDictionary<string, QlogValue> target, object? value, string description)
    {
        target.Clear();
        foreach (KeyValuePair<string, QlogValue> entry in ReadObjectMap(value, description))
        {
            target[entry.Key] = entry.Value;
        }
    }

    private static Dictionary<string, object?> RequireMap(object? value, string description)
    {
        if (value is not Dictionary<string, object?> map)
        {
            throw new InvalidOperationException($"Contained qlog {description} must be a CBOR map.");
        }

        return map;
    }

    private static List<object?> RequireArray(object? value, string description)
    {
        if (value is not List<object?> list)
        {
            throw new InvalidOperationException($"Contained qlog {description} must be a CBOR array.");
        }

        return list;
    }

    private static object? ReadValue(CborReader reader)
    {
        return reader.PeekState() switch
        {
            CborReaderState.StartMap => ReadMap(reader),
            CborReaderState.StartArray => ReadArray(reader),
            CborReaderState.TextString => reader.ReadTextString(),
            CborReaderState.Boolean => reader.ReadBoolean(),
            CborReaderState.Null => ReadNull(reader),
            CborReaderState.UnsignedInteger => reader.ReadUInt64(),
            CborReaderState.NegativeInteger => reader.ReadInt64(),
            CborReaderState.SinglePrecisionFloat => (double)reader.ReadSingle(),
            CborReaderState.DoublePrecisionFloat => reader.ReadDouble(),
            CborReaderState.HalfPrecisionFloat => (double)reader.ReadHalf(),
            CborReaderState.StartIndefiniteLengthTextString
                or CborReaderState.StartIndefiniteLengthByteString
                or CborReaderState.EndIndefiniteLengthByteString
                or CborReaderState.EndIndefiniteLengthTextString
                or CborReaderState.Tag
                or CborReaderState.SimpleValue
                or CborReaderState.Undefined
                or CborReaderState.Finished
                or CborReaderState.EndArray
                or CborReaderState.EndMap => throw new InvalidOperationException(
                    $"Unsupported CBOR reader state '{reader.PeekState()}'."),
            _ => throw new InvalidOperationException($"Unsupported CBOR reader state '{reader.PeekState()}'."),
        };
    }

    private static Dictionary<string, object?> ReadMap(CborReader reader)
    {
        int? length = reader.ReadStartMap();
        Dictionary<string, object?> values = new(StringComparer.Ordinal);

        if (length.HasValue)
        {
            for (int index = 0; index < length.Value; index++)
            {
                values[reader.ReadTextString()] = ReadValue(reader);
            }
        }
        else
        {
            while (reader.PeekState() is not CborReaderState.EndMap)
            {
                values[reader.ReadTextString()] = ReadValue(reader);
            }
        }

        reader.ReadEndMap();
        return values;
    }

    private static List<object?> ReadArray(CborReader reader)
    {
        int? length = reader.ReadStartArray();
        List<object?> values = new();

        if (length.HasValue)
        {
            values.Capacity = length.Value;
            for (int index = 0; index < length.Value; index++)
            {
                values.Add(ReadValue(reader));
            }
        }
        else
        {
            while (reader.PeekState() is not CborReaderState.EndArray)
            {
                values.Add(ReadValue(reader));
            }
        }

        reader.ReadEndArray();
        return values;
    }

    private static object? ReadNull(CborReader reader)
    {
        reader.ReadNull();
        return null;
    }

    private static QlogValue ReadQlogValue(object? value)
    {
        return value switch
        {
            null => QlogValue.Null,
            string text => QlogValue.FromString(text),
            bool boolean => QlogValue.FromBoolean(boolean),
            long signed => QlogValue.FromNumber(signed),
            ulong unsigned => QlogValue.FromNumber(unsigned),
            BigInteger bigInteger => ReadBigIntegerValue(bigInteger),
            double floatingPoint => ReadDoubleValue(floatingPoint),
            Dictionary<string, object?> map => QlogValue.FromObject(ReadObjectProperties(map)),
            List<object?> list => QlogValue.FromArray(ReadArrayValues(list)),
            _ => throw new InvalidOperationException(
                $"Unsupported CBOR value type '{value.GetType().FullName}'."),
        };
    }

    private static IEnumerable<KeyValuePair<string, QlogValue>> ReadObjectProperties(Dictionary<string, object?> map)
    {
        List<KeyValuePair<string, QlogValue>> properties = new(map.Count);
        foreach (KeyValuePair<string, object?> entry in map)
        {
            properties.Add(new KeyValuePair<string, QlogValue>(entry.Key, ReadQlogValue(entry.Value)));
        }

        return properties;
    }

    private static IEnumerable<QlogValue> ReadArrayValues(List<object?> list)
    {
        List<QlogValue> values = new(list.Count);
        foreach (object? entry in list)
        {
            values.Add(ReadQlogValue(entry));
        }

        return values;
    }

    private static QlogValue ReadBigIntegerValue(BigInteger value)
    {
        if (value.Sign < 0)
        {
            if (value >= long.MinValue)
            {
                return QlogValue.FromNumber((long)value);
            }
        }
        else if (value <= ulong.MaxValue)
        {
            return QlogValue.FromNumber((ulong)value);
        }

        return QlogValue.Parse(value.ToString(CultureInfo.InvariantCulture));
    }

    private static QlogValue ReadDoubleValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new InvalidOperationException("Contained qlog CBOR input cannot represent non-finite numbers.");
        }

        return QlogValue.FromNumber(value);
    }

    private static double ReadFiniteDouble(object? value, string description)
    {
        double numericValue = value switch
        {
            double floatingPoint => floatingPoint,
            long signed => signed,
            ulong unsigned => unsigned,
            BigInteger bigInteger => (double)bigInteger,
            _ => throw new InvalidOperationException($"Contained qlog {description} must be numeric."),
        };

        if (double.IsNaN(numericValue) || double.IsInfinity(numericValue))
        {
            throw new InvalidOperationException($"Contained qlog {description} must be finite.");
        }

        return numericValue;
    }

    private static string ReadString(object? value, string description)
    {
        return ReadNullableString(value, description)
            ?? throw new InvalidOperationException($"Contained qlog {description} must be a text string.");
    }

    private static string? ReadNullableString(object? value, string description)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string text)
        {
            return text;
        }

        throw new InvalidOperationException($"Contained qlog {description} must be a text string.");
    }

    private static Uri ReadAbsoluteUri(object? value, string description)
    {
        string text = ReadString(value, description);
        if (!Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
        {
            throw new InvalidOperationException($"Contained qlog {description} must be an absolute URI.");
        }

        return uri;
    }
}
