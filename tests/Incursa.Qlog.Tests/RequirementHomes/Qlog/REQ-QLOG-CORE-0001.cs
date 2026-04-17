using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_CORE_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S3-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S7-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P1-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P1-0002")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesContainedRootMetadataAndRoundTripsTheDraftShape()
    {
        string json = CreateContainedDraftJson();

        QlogFile parsed = QlogJsonSerializer.Deserialize(json);

        Assert.Equal(new Uri("urn:ietf:params:qlog:file:contained"), parsed.FileSchema);
        Assert.Equal("application/qlog+json", parsed.SerializationFormat);
        Assert.Equal("Name of this particular qlog file (short)", parsed.Title);
        Assert.Equal("Description for this group of traces (long)", parsed.Description);
        Assert.Equal("file_extension", parsed.ExtensionData.Keys.Single());
        Assert.Equal(2, parsed.Traces.Count);

        QlogTrace trace = Assert.IsType<QlogTrace>(parsed.Traces[0]);
        Assert.Equal("Name of this particular trace (short)", trace.Title);
        Assert.Equal("Description for this trace (long)", trace.Description);
        Assert.Equal("trace_extension", trace.ExtensionData.Keys.Single());
        Assert.Single(trace.EventSchemas, new Uri("urn:ietf:params:qlog:events:quic"));

        QlogCommonFields commonFields = Assert.IsType<QlogCommonFields>(trace.CommonFields);
        Assert.Equal(QlogValue.FromString("abcde1234"), commonFields.ExtensionData["ODCID"]);
        Assert.Equal("relative_to_epoch", commonFields.TimeFormat);
        Assert.Equal("trace-group", commonFields.GroupId);
        Assert.Equal("203.0.113.1:443-198.51.100.1:443", commonFields.Tuple);

        QlogReferenceTime referenceTime = Assert.IsType<QlogReferenceTime>(commonFields.ReferenceTime);
        Assert.Equal("system", referenceTime.ClockType);
        Assert.Equal("1970-01-01T00:00:00.000Z", referenceTime.Epoch);

        QlogVantagePoint vantagePoint = Assert.IsType<QlogVantagePoint>(trace.VantagePoint);
        Assert.Equal("backend-67", vantagePoint.Name);
        Assert.Equal("server", vantagePoint.Type);
        Assert.Equal(QlogValue.FromString("prod"), vantagePoint.ExtensionData["environment"]);

        QlogEvent qlogEvent = Assert.Single(trace.Events);
        Assert.Equal(2, qlogEvent.Time);
        Assert.Equal("quic:packet_received", qlogEvent.Name);
        Assert.Equal("trace-group", qlogEvent.GroupId);
        Assert.Equal(QlogValue.FromString("kept"), qlogEvent.ExtensionData["event_extension"]);
        Assert.Equal(QlogValue.FromNumber(1280), qlogEvent.Data["packet_size"]);
        Assert.Equal(QlogValue.FromBoolean(true), qlogEvent.Data["has_ack"]);
        Assert.Equal(QlogValue.Parse("""{"packet_type":"1RTT","packet_number":123}"""), qlogEvent.Data["header"]);

        QlogTraceError traceError = Assert.IsType<QlogTraceError>(parsed.Traces[1]);
        Assert.Equal("File could not be found", traceError.ErrorDescription);
        Assert.Equal("/srv/traces/today/latest.qlog", traceError.Uri);
        Assert.Equal("server", traceError.VantagePoint?.Type);

        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);
        Assert.Equal(json, roundTrippedJson);

        JsonNode? originalNode = JsonNode.Parse(json);
        JsonNode? roundTrippedNode = JsonNode.Parse(roundTrippedJson);
        Assert.True(JsonNode.DeepEquals(originalNode, roundTrippedNode));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S4-0001")]
    [Trait("CoverageType", "Negative")]
    public void Serialize_RejectsContainedFilesWithoutTraceComponents()
    {
        QlogFile file = new();

        Assert.Throws<InvalidOperationException>(() => QlogJsonSerializer.Serialize(file));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P2-0003")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_PreservesPrivateEventSchemaUrisAndRoundTripsTheDraftShape()
    {
        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.ContainedFileSchemaUri,
            SerializationFormat = QlogKnownValues.ContainedJsonSerializationFormat,
        };

        QlogTrace trace = new();
        trace.EventSchemas.Add(new Uri("urn:example:qlog:events:private-foo"));

        QlogEvent qlogEvent = new()
        {
            Time = 1,
            Name = "custom:event",
        };
        qlogEvent.Data["marker"] = QlogValue.FromString("kept");
        trace.Events.Add(qlogEvent);
        file.Traces.Add(trace);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);

        Assert.Single(parsed.Traces);
        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces[0]);
        Assert.Equal(new Uri("urn:example:qlog:events:private-foo"), Assert.Single(parsedTrace.EventSchemas));
        Assert.Equal(QlogValue.FromString("kept"), Assert.Single(parsedTrace.Events).Data["marker"]);
        Assert.Equal(json, QlogJsonSerializer.Serialize(parsed));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S9-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_StreamOverload_WritesContainedJsonEquivalentToStringOverload()
    {
        QlogFile file = new();
        QlogTrace trace = new()
        {
            Title = "stream-contained-trace",
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.Events.Add(new QlogEvent
        {
            Time = 0,
            Name = "example:stream_write",
        });
        file.Traces.Add(trace);

        string expected = QlogJsonSerializer.Serialize(file, indented: false);

        string actual;
        using (MemoryStream stream = new())
        {
            QlogJsonSerializer.Serialize(stream, file, indented: false);
            actual = Encoding.UTF8.GetString(stream.ToArray());
        }

        Assert.Equal(expected, actual);
        Assert.StartsWith("{\"file_schema\":\"urn:ietf:params:qlog:file:contained\"", actual, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S6-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P2-0002")]
    [Trait("CoverageType", "Negative")]
    public void Serialize_RejectsTracesWithoutEventSchemas()
    {
        QlogFile file = new();
        file.Traces.Add(new QlogTrace());

        Assert.Throws<InvalidOperationException>(() => QlogJsonSerializer.Serialize(file));
    }

    private static string CreateContainedDraftJson()
    {
        return """
{"file_schema":"urn:ietf:params:qlog:file:contained","serialization_format":"application/qlog+json","title":"Name of this particular qlog file (short)","description":"Description for this group of traces (long)","file_extension":1,"traces":[{"title":"Name of this particular trace (short)","description":"Description for this trace (long)","common_fields":{"tuple":"203.0.113.1:443-198.51.100.1:443","time_format":"relative_to_epoch","group_id":"trace-group","reference_time":{"clock_type":"system","epoch":"1970-01-01T00:00:00.000Z"},"ODCID":"abcde1234"},"vantage_point":{"name":"backend-67","type":"server","environment":"prod"},"event_schemas":["urn:ietf:params:qlog:events:quic"],"events":[{"time":2,"name":"quic:packet_received","data":{"packet_size":1280,"header":{"packet_type":"1RTT","packet_number":123},"has_ack":true},"tuple":"203.0.113.1:443-198.51.100.1:443","time_format":"time_overrides_common_fields","group_id":"trace-group","event_extension":"kept"}],"trace_extension":"kept"},{"error_description":"File could not be found","uri":"/srv/traces/today/latest.qlog","vantage_point":{"type":"server"}}]}
""";
    }
}
