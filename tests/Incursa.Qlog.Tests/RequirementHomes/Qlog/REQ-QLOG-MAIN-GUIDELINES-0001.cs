using System.Text.Json;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests.RequirementHomes.Qlog;

public sealed class REQ_QLOG_MAIN_GUIDELINES_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S1P2-0001")]
    [Trait("CoverageType", "Positive")]
    public void Deserialize_TreatsContainedJsonMemberOrderAsInsignificant()
    {
        const string Json = """
{"traces":[{"events":[{"data":{"message":"connected"},"name":"loglevel:info","time":1}],"event_schemas":["urn:ietf:params:qlog:events:loglevel"],"title":"ordered differently"}],"description":"fields are intentionally out of canonical order","serialization_format":"application/qlog+json","file_schema":"urn:ietf:params:qlog:file:contained"}
""";

        QlogFile parsed = QlogJsonSerializer.Deserialize(Json);

        Assert.Equal(QlogKnownValues.ContainedFileSchemaUri, parsed.FileSchema);
        Assert.Equal(QlogKnownValues.ContainedJsonSerializationFormat, parsed.SerializationFormat);
        QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(parsed.Traces));
        QlogEvent qlogEvent = Assert.Single(trace.Events);
        Assert.Equal("loglevel:info", qlogEvent.Name);
        Assert.Equal(QlogValue.FromString("connected"), qlogEvent.Data["message"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S3P1-0001")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S11P1-0002")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesContainedJsonIdentifierFieldsFirstAndLowercaseQlogNames()
    {
        QlogFile file = CreateFileWithGenericEvents();

        string json = QlogJsonSerializer.Serialize(file);

        Assert.StartsWith(
            "{\"file_schema\":\"urn:ietf:params:qlog:file:contained\",\"serialization_format\":\"application/qlog+json\"",
            json,
            StringComparison.Ordinal);

        using JsonDocument document = JsonDocument.Parse(json);
        AssertAllPropertyNamesLowercase(document.RootElement);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S9-0002")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S9-0003")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_PreservesGenericLoglevelAndSimulationEvents()
    {
        QlogFile file = CreateFileWithGenericEvents();

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);

        QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(parsed.Traces));
        Assert.Contains(new Uri("urn:ietf:params:qlog:events:loglevel"), trace.EventSchemas);
        Assert.Contains(new Uri("urn:ietf:params:qlog:events:simulation"), trace.EventSchemas);

        QlogEvent loglevel = Assert.Single(trace.Events, qlogEvent => qlogEvent.Name == "loglevel:error");
        Assert.Equal(QlogValue.FromString("connection failed"), loglevel.Data["message"]);

        QlogEvent marker = Assert.Single(trace.Events, qlogEvent => qlogEvent.Name == "simulation:marker");
        Assert.Equal(QlogValue.FromString("handoff"), marker.Data["name"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S11P3-0001")]
    [Trait("CoverageType", "Positive")]
    public void Deserialize_PreservesUint64ValuesEncodedAsJsonStringsOrNumbers()
    {
        const string Json = """
{"file_schema":"urn:ietf:params:qlog:file:contained","serialization_format":"application/qlog+json","traces":[{"event_schemas":["urn:ietf:params:qlog:events:example"],"events":[{"time":0,"name":"example:uint64_values","data":{"packet_number_as_string":"18446744073709551615","packet_number_as_number":9007199254740991}}]}]}
""";

        QlogFile parsed = QlogJsonSerializer.Deserialize(Json);

        QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(parsed.Traces));
        QlogEvent qlogEvent = Assert.Single(trace.Events);
        Assert.Equal(QlogValue.FromString("18446744073709551615"), qlogEvent.Data["packet_number_as_string"]);
        Assert.Equal(QlogValue.FromNumber(9007199254740991), qlogEvent.Data["packet_number_as_number"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S13-0001")]
    [Trait("CoverageType", "Positive")]
    public void Deserialize_PreservesUnknownFieldsAndUnsupportedEvents()
    {
        const string Json = """
{"file_schema":"urn:ietf:params:qlog:file:contained","serialization_format":"application/qlog+json","file_unknown":"kept","traces":[{"event_schemas":["urn:ietf:params:qlog:events:example"],"trace_unknown":true,"common_fields":{"common_unknown":"kept"},"events":[{"time":0,"name":"example:unknown_event","data":{"payload_unknown":{"nested":1}},"event_unknown":"kept"}]}]}
""";

        QlogFile parsed = QlogJsonSerializer.Deserialize(Json);

        Assert.Equal(QlogValue.FromString("kept"), parsed.ExtensionData["file_unknown"]);
        QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(parsed.Traces));
        Assert.Equal(QlogValue.FromBoolean(true), trace.ExtensionData["trace_unknown"]);
        Assert.Equal(QlogValue.FromString("kept"), trace.CommonFields?.ExtensionData["common_unknown"]);
        QlogEvent qlogEvent = Assert.Single(trace.Events);
        Assert.Equal("example:unknown_event", qlogEvent.Name);
        Assert.Equal(QlogValue.Parse("""{"nested":1}"""), qlogEvent.Data["payload_unknown"]);
        Assert.Equal(QlogValue.FromString("kept"), qlogEvent.ExtensionData["event_unknown"]);
    }

    [Fact(Skip = "Requirement is captured for the next schema-description surface; the current library has no public event-schema metadata model.")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P3-0001")]
    [Trait("CoverageType", "Positive")]
    public void EventSchemaSurface_PreservesEventImportanceMetadata()
    {
    }

    [Fact(Skip = "Requirement is captured for the next file-generation surface; low-level serializers intentionally do not read process environment variables.")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S12P1-0001")]
    [Trait("CoverageType", "Positive")]
    public void FileGenerationSurface_HonorsQlogDirDirectorySelection()
    {
    }

    [Fact(Skip = "Requirement is captured for the next capture-policy surface; operator access control and retention remain outside the serializer boundary.")]
    [Trait("Requirement", "REQ-QLOG-MAIN-S14-0001")]
    [Trait("CoverageType", "Positive")]
    public void CaptureSurface_RequiresExplicitCallerActionBeforeWriting()
    {
    }

    private static QlogFile CreateFileWithGenericEvents()
    {
        QlogTrace trace = new()
        {
            Title = "generic events",
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:loglevel"));
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:simulation"));

        QlogEvent loglevel = new()
        {
            Time = 0,
            Name = "loglevel:error",
        };
        loglevel.Data["message"] = QlogValue.FromString("connection failed");
        trace.Events.Add(loglevel);

        QlogEvent marker = new()
        {
            Time = 1,
            Name = "simulation:marker",
        };
        marker.Data["name"] = QlogValue.FromString("handoff");
        trace.Events.Add(marker);

        QlogFile file = new();
        file.Traces.Add(trace);
        return file;
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
                foreach (JsonElement item in element.EnumerateArray())
                {
                    AssertAllPropertyNamesLowercase(item);
                }

                break;
        }
    }
}
