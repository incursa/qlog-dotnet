using System.Formats.Cbor;
using System.IO;
using Incursa.Qlog.Serialization.Cbor;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_CBOR_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0004")]
    [Trait("CoverageType", "Positive")]
    public void Serializer_LivesInTheSiblingCborPackage()
    {
        Assert.Equal("Incursa.Qlog.Cbor", typeof(QlogCborSerializer).Assembly.GetName().Name);
        Assert.Equal("Incursa.Qlog.Serialization.Cbor", typeof(QlogCborSerializer).Namespace);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0006")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0007")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesContainedCborUsingTheSelectedArtifactPolicy()
    {
        QlogFile file = new()
        {
            SerializationFormat = QlogCborKnownValues.ContainedCborSerializationFormat,
            Title = "contained-cbor-file",
            Description = "first cbor slice",
        };
        file.ExtensionData["file_extension"] = QlogValue.FromString(QlogCborKnownValues.ContainedCborFileExtension);

        QlogTrace trace = new()
        {
            Title = "contained-cbor-trace",
            CommonFields = new QlogCommonFields
            {
                GroupId = "cbor-group",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
            VantagePoint = new QlogVantagePoint
            {
                Type = QlogKnownValues.ClientVantagePoint,
            },
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.ExtensionData["trace_extension"] = QlogValue.FromArray(
            new[]
            {
                QlogValue.FromString("alpha"),
                QlogValue.FromNumber(2),
            });

        QlogEvent qlogEvent = new()
        {
            Time = 12.5,
            Name = "example:binary_serialized",
            GroupId = "cbor-group",
        };
        qlogEvent.Data["packet_number"] = QlogValue.FromNumber(7);
        qlogEvent.Data["header"] = QlogValue.Parse("""{"packet_type":"1RTT","ack_eliciting":true}""");
        qlogEvent.ExtensionData["event_extension"] = QlogValue.FromString("kept");
        trace.Events.Add(qlogEvent);
        file.Traces.Add(trace);

        byte[] payload = QlogCborSerializer.Serialize(file);

        Assert.NotEmpty(payload);

        CborReader reader = new(payload, CborConformanceMode.Strict);
        var root = RequireMap(ReadValue(reader));

        Assert.Equal(QlogKnownValues.ContainedFileSchemaUri.OriginalString, RequireString(root["file_schema"]));
        Assert.Equal(QlogCborKnownValues.ContainedCborSerializationFormat, RequireString(root["serialization_format"]));
        Assert.Equal(QlogCborKnownValues.ContainedCborFileExtension, RequireString(root["file_extension"]));

        var traces = RequireList(root["traces"]);
        Assert.Single(traces);

        var encodedTrace = RequireMap(traces[0]);
        Assert.Equal("contained-cbor-trace", RequireString(encodedTrace["title"]));
        Assert.Equal(
            new Uri("urn:ietf:params:qlog:events:example"),
            new Uri(RequireString(RequireList(encodedTrace["event_schemas"]).Single())));

        var encodedEvent = RequireMap(RequireList(encodedTrace["events"]).Single());
        Assert.Equal("example:binary_serialized", RequireString(encodedEvent["name"]));
        Assert.Equal(12.5d, Assert.IsType<double>(encodedEvent["time"]));

        var encodedData = RequireMap(encodedEvent["data"]);
        Assert.Equal(7L, Assert.IsType<long>(encodedData["packet_number"]));

        var encodedHeader = RequireMap(encodedData["header"]);
        Assert.Equal("1RTT", RequireString(encodedHeader["packet_type"]));
        Assert.True(Assert.IsType<bool>(encodedHeader["ack_eliciting"]));

        Assert.Equal(0, reader.BytesRemaining);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0003")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_StreamOverload_WritesTheSamePayloadAsTheArrayOverload()
    {
        QlogFile file = CreateMinimalCborFile();

        byte[] expected = QlogCborSerializer.Serialize(file);

        byte[] actual;
        using (MemoryStream stream = new())
        {
            QlogCborSerializer.Serialize(stream, file);
            actual = stream.ToArray();
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-CBOR-S1-0006")]
    [Trait("CoverageType", "Negative")]
    public void Serialize_RejectsFilesThatDoNotMatchTheContainedCborArtifactPolicy()
    {
        QlogFile file = CreateMinimalCborFile();
        file.FileSchema = QlogKnownValues.SequentialFileSchemaUri;
        file.SerializationFormat = QlogKnownValues.ContainedJsonSerializationFormat;

        Assert.Throws<InvalidOperationException>(() => QlogCborSerializer.Serialize(file));
    }

    private static QlogFile CreateMinimalCborFile()
    {
        QlogFile file = new()
        {
            SerializationFormat = QlogCborKnownValues.ContainedCborSerializationFormat,
        };

        QlogTrace trace = new();
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.Events.Add(new QlogEvent
        {
            Time = 0,
            Name = "example:minimal",
        });

        file.Traces.Add(trace);
        return file;
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
            CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger => reader.ReadInt64(),
            CborReaderState.SinglePrecisionFloat => (double)reader.ReadSingle(),
            CborReaderState.DoublePrecisionFloat => reader.ReadDouble(),
            CborReaderState.HalfPrecisionFloat => (double)reader.ReadHalf(),
            _ => throw new InvalidOperationException($"Unsupported reader state '{reader.PeekState()}'."),
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
                values.Add(reader.ReadTextString(), ReadValue(reader));
            }
        }
        else
        {
            while (reader.PeekState() is not CborReaderState.EndMap)
            {
                values.Add(reader.ReadTextString(), ReadValue(reader));
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

    private static Dictionary<string, object?> RequireMap(object? value) =>
        Assert.IsType<Dictionary<string, object?>>(value);

    private static List<object?> RequireList(object? value) =>
        Assert.IsType<List<object?>>(value);

    private static string RequireString(object? value) =>
        Assert.IsType<string>(value);
}
