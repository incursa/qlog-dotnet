// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
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

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S8P3-0001")]
    [Trait("CoverageType", "Positive")]
    public void EventSchemaSurface_PreservesEventImportanceMetadata()
    {
        Assert.Equal(
            new Uri("urn:ietf:params:qlog:events:loglevel"),
            QlogKnownEventSchemas.LogLevel.SchemaUri);
        Assert.Equal(
            new[]
            {
                "loglevel:error|Core",
                "loglevel:warning|Base",
                "loglevel:info|Extra",
                "loglevel:debug|Extra",
                "loglevel:verbose|Extra",
            },
            QlogKnownEventSchemas.LogLevel.EventDefinitions.Select(static definition =>
                $"{definition.Name}|{definition.ImportanceLevel}"));

        Assert.Equal(
            new Uri("urn:ietf:params:qlog:events:simulation"),
            QlogKnownEventSchemas.Simulation.SchemaUri);
        Assert.Equal(
            new[]
            {
                "simulation:scenario|Extra",
                "simulation:marker|Extra",
            },
            QlogKnownEventSchemas.Simulation.EventDefinitions.Select(static definition =>
                $"{definition.Name}|{definition.ImportanceLevel}"));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S12P1-0001")]
    [Trait("CoverageType", "Positive")]
    public async Task FileGenerationSurface_HonorsQlogDirDirectorySelection()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string qlogDirectory = Path.Combine(root, "qlogs");
        string targetPath = Path.Combine(qlogDirectory, "generated.qlog");
        string? previousQlogDir = Environment.GetEnvironmentVariable("QLOGDIR");
        string? previousQlogFile = Environment.GetEnvironmentVariable("QLOGFILE");

        try
        {
            Environment.SetEnvironmentVariable("QLOGDIR", qlogDirectory + Path.DirectorySeparatorChar);
            Environment.SetEnvironmentVariable("QLOGFILE", null);

            await using QlogCaptureCoordinator coordinator = new();
            QlogCaptureSession session = CreateSession("qlogdir-session");

            await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
                session,
                new QlogFileCaptureSink("generated.qlog"));

            coordinator.Capture(session, CreateEvent("example:qlogdir", 1));
            await coordinator.CompleteSessionAsync(session);

            string output = await File.ReadAllTextAsync(targetPath);
            Assert.StartsWith("\u001e{\"file_schema\":\"urn:ietf:params:qlog:file:sequential\"", output, StringComparison.Ordinal);
            Assert.Contains("\"name\":\"example:qlogdir\"", output, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("QLOGDIR", previousQlogDir);
            Environment.SetEnvironmentVariable("QLOGFILE", previousQlogFile);
        }
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S14-0001")]
    [Trait("CoverageType", "Negative")]
    public async Task CaptureSurface_RejectsWritingBeforeCaptureIsExplicitlyStarted()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "capture-policy.qlog");

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateInactiveSession("capture-policy-session");

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogFileCaptureSink(targetPath));

        Assert.Throws<InvalidOperationException>(() => coordinator.Capture(session, CreateEvent("example:policy", 1)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => coordinator.CompleteSessionAsync(session).AsTask());
        Assert.False(File.Exists(targetPath));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S14-0001")]
    [Trait("CoverageType", "Positive")]
    public async Task CaptureSurface_WritesAfterAnExplicitStartAction()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "capture-policy-started.qlog");

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("capture-policy-started");

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogFileCaptureSink(targetPath));

        coordinator.Capture(session, CreateEvent("example:policy_started", 2));
        await coordinator.CompleteSessionAsync(session);

        string output = await File.ReadAllTextAsync(targetPath);
        Assert.Contains("\"name\":\"example:policy_started\"", output, StringComparison.Ordinal);
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

    private static QlogCaptureSession CreateSession(string sessionId)
    {
        QlogTrace trace = new()
        {
            Title = "main-guidelines-trace",
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));

        QlogCaptureSession session = new(sessionId, trace, fileTitle: "main-guidelines-file");
        session.StartCapture();
        return session;
    }

    private static QlogCaptureSession CreateInactiveSession(string sessionId)
    {
        QlogTrace trace = new()
        {
            Title = "main-guidelines-trace",
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));

        return new QlogCaptureSession(sessionId, trace, fileTitle: "main-guidelines-file");
    }

    private static QlogEvent CreateEvent(string name, int packetSize)
    {
        QlogEvent qlogEvent = new()
        {
            Time = packetSize,
            Name = name,
        };
        qlogEvent.Data["packet_size"] = QlogValue.FromNumber(packetSize);
        return qlogEvent;
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
