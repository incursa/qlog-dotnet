using System.Text.Json;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_SEQUENTIAL_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S5-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S11P2-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesASequentialHeaderRecordAndEventRecordsWithLowercaseFieldNames()
    {
        QlogFile file = CreateSequentialDraftFile(includeEvents: true);

        string jsonTextSequence = QlogJsonTextSequenceSerializer.Serialize(file);

        Assert.Equal('\u001e', jsonTextSequence[0]);
        Assert.Equal('\n', jsonTextSequence[^1]);

        string[] records = jsonTextSequence.Split('\u001e', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, records.Length);

        using JsonDocument headerDocument = JsonDocument.Parse(records[0]);
        using JsonDocument eventDocument = JsonDocument.Parse(records[1]);

        AssertAllPropertyNamesLowercase(headerDocument.RootElement);
        AssertAllPropertyNamesLowercase(eventDocument.RootElement);

        JsonElement header = headerDocument.RootElement;
        Assert.Equal("urn:ietf:params:qlog:file:sequential", header.GetProperty("file_schema").GetString());
        Assert.Equal("application/qlog+json-seq", header.GetProperty("serialization_format").GetString());
        Assert.Equal("Name of this particular qlog file (short)", header.GetProperty("title").GetString());
        Assert.Equal("Description for this group of traces (long)", header.GetProperty("description").GetString());
        Assert.Equal(1, header.GetProperty("file_extension").GetInt32());

        JsonElement trace = header.GetProperty("trace");
        Assert.Equal("Name of this particular trace (short)", trace.GetProperty("title").GetString());
        Assert.Equal("Description for this trace (long)", trace.GetProperty("description").GetString());
        Assert.Equal("kept", trace.GetProperty("trace_extension").GetString());
        Assert.False(trace.TryGetProperty("events", out _));
        JsonElement eventSchemas = trace.GetProperty("event_schemas");
        JsonElement eventSchema = Assert.Single(eventSchemas.EnumerateArray());
        Assert.Equal("urn:ietf:params:qlog:events:quic", eventSchema.GetString());

        JsonElement commonFields = trace.GetProperty("common_fields");
        Assert.Equal("203.0.113.1:443-198.51.100.1:443", commonFields.GetProperty("tuple").GetString());
        Assert.Equal("relative_to_epoch", commonFields.GetProperty("time_format").GetString());
        Assert.Equal("trace-group", commonFields.GetProperty("group_id").GetString());
        Assert.Equal("kept", commonFields.GetProperty("common_fields_extension").GetString());

        JsonElement referenceTime = commonFields.GetProperty("reference_time");
        Assert.Equal("system", referenceTime.GetProperty("clock_type").GetString());
        Assert.Equal("1970-01-01T00:00:00.000Z", referenceTime.GetProperty("epoch").GetString());

        JsonElement vantagePoint = trace.GetProperty("vantage_point");
        Assert.Equal("backend-67", vantagePoint.GetProperty("name").GetString());
        Assert.Equal("server", vantagePoint.GetProperty("type").GetString());

        JsonElement eventRecord = eventDocument.RootElement;
        Assert.Equal(2, eventRecord.GetProperty("time").GetInt32());
        Assert.Equal("quic:packet_received", eventRecord.GetProperty("name").GetString());
        Assert.Equal("203.0.113.1:443-198.51.100.1:443", eventRecord.GetProperty("tuple").GetString());
        Assert.Equal("time_overrides_common_fields", eventRecord.GetProperty("time_format").GetString());
        Assert.Equal("trace-group", eventRecord.GetProperty("group_id").GetString());
        Assert.Equal("kept", eventRecord.GetProperty("event_extension").GetString());

        JsonElement eventData = eventRecord.GetProperty("data");
        Assert.Equal(1280, eventData.GetProperty("packet_size").GetInt32());
        Assert.True(eventData.GetProperty("has_ack").GetBoolean());
        JsonElement headerData = eventData.GetProperty("header");
        Assert.Equal("1RTT", headerData.GetProperty("packet_type").GetString());
        Assert.Equal(123, headerData.GetProperty("packet_number").GetInt32());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S5-0001")]
    [Trait("CoverageType", "Edge")]
    public void Serialize_WritesAHeaderOnlySequentialRecordWhenTheTraceHasNoEvents()
    {
        QlogFile file = CreateSequentialDraftFile(includeEvents: false);

        string jsonTextSequence = QlogJsonTextSequenceSerializer.Serialize(file);

        Assert.Equal('\u001e', jsonTextSequence[0]);
        Assert.Equal('\n', jsonTextSequence[^1]);

        string[] records = jsonTextSequence.Split('\u001e', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(records);

        using JsonDocument headerDocument = JsonDocument.Parse(records[0]);
        AssertAllPropertyNamesLowercase(headerDocument.RootElement);

        JsonElement trace = headerDocument.RootElement.GetProperty("trace");
        Assert.False(trace.TryGetProperty("events", out _));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S5-0001")]
    [Trait("CoverageType", "Negative")]
    public void Serialize_RejectsSequentialFilesWithMoreThanOneTraceComponent()
    {
        QlogFile file = CreateSequentialDraftFile(includeEvents: false);
        file.Traces.Add(CreateSequentialTrace());

        Assert.Throws<InvalidOperationException>(() => QlogJsonTextSequenceSerializer.Serialize(file));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S5-0001")]
    [Trait("CoverageType", "Negative")]
    public void Serialize_RejectsSequentialFilesWithTraceErrorComponents()
    {
        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
        };

        file.Traces.Add(new QlogTraceError
        {
            ErrorDescription = "File could not be found",
        });

        Assert.Throws<InvalidOperationException>(() => QlogJsonTextSequenceSerializer.Serialize(file));
    }

    private static QlogFile CreateSequentialDraftFile(bool includeEvents)
    {
        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
            Title = "Name of this particular qlog file (short)",
            Description = "Description for this group of traces (long)",
        };

        file.ExtensionData["file_extension"] = QlogValue.FromNumber(1);
        file.Traces.Add(CreateSequentialTrace(includeEvents));
        return file;
    }

    private static QlogTrace CreateSequentialTrace(bool includeEvents = true)
    {
        QlogTrace trace = new()
        {
            Title = "Name of this particular trace (short)",
            Description = "Description for this trace (long)",
            CommonFields = new QlogCommonFields
            {
                Tuple = "203.0.113.1:443-198.51.100.1:443",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
                GroupId = "trace-group",
                ReferenceTime = new QlogReferenceTime
                {
                    ClockType = QlogKnownValues.SystemClockType,
                    Epoch = "1970-01-01T00:00:00.000Z",
                },
            },
            VantagePoint = new QlogVantagePoint
            {
                Name = "backend-67",
                Type = QlogKnownValues.ServerVantagePoint,
            },
        };

        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:quic"));
        trace.ExtensionData["trace_extension"] = QlogValue.FromString("kept");
        trace.CommonFields.ExtensionData["common_fields_extension"] = QlogValue.FromString("kept");

        if (includeEvents)
        {
            QlogEvent qlogEvent = new()
            {
                Time = 2,
                Name = "quic:packet_received",
                Tuple = "203.0.113.1:443-198.51.100.1:443",
                TimeFormat = "time_overrides_common_fields",
                GroupId = "trace-group",
            };

            qlogEvent.Data["packet_size"] = QlogValue.FromNumber(1280);
            qlogEvent.Data["has_ack"] = QlogValue.FromBoolean(true);
            qlogEvent.Data["header"] = QlogValue.Parse("""{"packet_type":"1RTT","packet_number":123}""");
            qlogEvent.ExtensionData["event_extension"] = QlogValue.FromString("kept");

            trace.Events.Add(qlogEvent);
        }

        return trace;
    }

    private static void AssertAllPropertyNamesLowercase(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    Assert.Equal(property.Name.ToLowerInvariant(), property.Name);
                    AssertAllPropertyNamesLowercase(property.Value);
                }

                break;
            case JsonValueKind.Array:
                foreach (JsonElement arrayItem in element.EnumerateArray())
                {
                    AssertAllPropertyNamesLowercase(arrayItem);
                }

                break;
        }
    }
}
