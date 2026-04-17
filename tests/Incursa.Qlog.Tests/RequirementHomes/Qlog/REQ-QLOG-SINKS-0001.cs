using System;
using System.Formats.Cbor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Incursa.Qlog.Serialization.Cbor;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_SINKS_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0005")]
    [Trait("CoverageType", "Positive")]
    public async Task FileSink_WritesSequentialOutputAndCreatesMissingDirectories()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture.qlog");

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("file-sink-session");
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, new QlogFileCaptureSink(targetPath));

        QlogEvent qlogEvent = CreateEvent("example:file_sink", 42);
        coordinator.Capture(session, qlogEvent);

        qlogEvent.Name = "example:mutated";
        qlogEvent.Data["packet_size"] = QlogValue.FromNumber(999);

        await coordinator.CompleteSessionAsync(session);

        string output = await File.ReadAllTextAsync(targetPath);
        Assert.StartsWith("\u001e{\"file_schema\":\"urn:ietf:params:qlog:file:sequential\"", output, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"example:file_sink\"", output, StringComparison.Ordinal);
        Assert.Contains("\"packet_size\":42", output, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0006")]
    [Trait("CoverageType", "Positive")]
    public async Task FileSink_CanWriteContainedJsonOutput()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture-contained.qlog");

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("file-contained-session");
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogFileCaptureSink(targetPath, QlogCaptureSinkFormat.ContainedJson));

        coordinator.Capture(session, CreateEvent("example:file_contained", 17));
        await coordinator.CompleteSessionAsync(session);

        string output = await File.ReadAllTextAsync(targetPath);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement rootElement = document.RootElement;
        Assert.Equal("urn:ietf:params:qlog:file:contained", rootElement.GetProperty("file_schema").GetString());
        Assert.Equal("application/qlog+json", rootElement.GetProperty("serialization_format").GetString());

        JsonElement trace = Assert.Single(rootElement.GetProperty("traces").EnumerateArray());
        JsonElement capturedEvent = Assert.Single(trace.GetProperty("events").EnumerateArray());
        Assert.Equal("example:file_contained", capturedEvent.GetProperty("name").GetString());
        Assert.Equal(17, capturedEvent.GetProperty("data").GetProperty("packet_size").GetInt32());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0008")]
    [Trait("CoverageType", "Positive")]
    public async Task FileSink_CanWriteContainedCborOutput()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture-contained.qlog.cbor");

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("file-cbor-session");
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogCborFileCaptureSink(targetPath));

        coordinator.Capture(session, CreateEvent("example:file_cbor", 19));
        await coordinator.CompleteSessionAsync(session);

        byte[] payload = await File.ReadAllBytesAsync(targetPath);
        CborReader reader = new(payload, CborConformanceMode.Strict);
        var rootMap = RequireMap(ReadCborValue(reader));

        Assert.Equal("urn:ietf:params:qlog:file:contained", RequireString(rootMap["file_schema"]));
        Assert.Equal(QlogCborKnownValues.ContainedCborSerializationFormat, RequireString(rootMap["serialization_format"]));

        var trace = RequireMap(RequireList(rootMap["traces"]).Single());
        var capturedEvent = RequireMap(RequireList(trace["events"]).Single());
        Assert.Equal("example:file_cbor", RequireString(capturedEvent["name"]));
        Assert.Equal(19L, Assert.IsType<long>(RequireMap(capturedEvent["data"])["packet_size"]));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0005")]
    [Trait("CoverageType", "Positive")]
    public async Task StreamSink_WritesSequentialOutputToTheProvidedStream()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("stream-sink-session");
        await using MemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, new QlogStreamCaptureSink(stream));

        coordinator.Capture(session, CreateEvent("example:stream_sink", 7));
        await coordinator.CompleteSessionAsync(session);

        string output = Encoding.UTF8.GetString(stream.ToArray());
        Assert.StartsWith("\u001e{\"file_schema\":\"urn:ietf:params:qlog:file:sequential\"", output, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"example:stream_sink\"", output, StringComparison.Ordinal);
        Assert.Contains("\"packet_size\":7", output, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0006")]
    [Trait("CoverageType", "Positive")]
    public async Task StreamSink_CanWriteContainedJsonOutput()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("stream-contained-session");
        await using MemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogStreamCaptureSink(stream, format: QlogCaptureSinkFormat.ContainedJson));

        coordinator.Capture(session, CreateEvent("example:stream_contained", 13));
        await coordinator.CompleteSessionAsync(session);

        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        JsonElement rootElement = document.RootElement;
        Assert.Equal("urn:ietf:params:qlog:file:contained", rootElement.GetProperty("file_schema").GetString());
        JsonElement trace = Assert.Single(rootElement.GetProperty("traces").EnumerateArray());
        JsonElement capturedEvent = Assert.Single(trace.GetProperty("events").EnumerateArray());
        Assert.Equal("example:stream_contained", capturedEvent.GetProperty("name").GetString());
        Assert.Equal(13, capturedEvent.GetProperty("data").GetProperty("packet_size").GetInt32());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0008")]
    [Trait("CoverageType", "Positive")]
    public async Task StreamSink_CanWriteContainedCborOutput()
    {
        await using QlogCaptureCoordinator coordinator = new();
        await using MemoryStream stream = new();
        QlogCaptureSession session = CreateSession("stream-cbor-session");
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(
            session,
            new QlogCborStreamCaptureSink(stream));

        coordinator.Capture(session, CreateEvent("example:stream_cbor", 23));
        await coordinator.CompleteSessionAsync(session);

        CborReader reader = new(stream.ToArray(), CborConformanceMode.Strict);
        var rootMap = RequireMap(ReadCborValue(reader));
        var trace = RequireMap(RequireList(rootMap["traces"]).Single());
        var capturedEvent = RequireMap(RequireList(trace["events"]).Single());

        Assert.Equal(QlogCborKnownValues.ContainedCborSerializationFormat, RequireString(rootMap["serialization_format"]));
        Assert.Equal("example:stream_cbor", RequireString(capturedEvent["name"]));
        Assert.Equal(23L, Assert.IsType<long>(RequireMap(capturedEvent["data"])["packet_size"]));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0007")]
    [Trait("CoverageType", "Positive")]
    public async Task ContainedFileSink_AggregatesMultipleSessionsThroughOneSerializedWriter()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture-contained-shared.qlog");

        await using QlogCaptureCoordinator coordinator = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(
            new QlogFileCaptureSink(targetPath, QlogCaptureSinkFormat.ContainedJson));

        QlogCaptureSession firstSession = CreateSession("contained-first-session");
        coordinator.Capture(firstSession, CreateEvent("example:first_session", 1));
        await coordinator.CompleteSessionAsync(firstSession);

        QlogCaptureSession secondSession = CreateSession("contained-second-session");
        coordinator.Capture(secondSession, CreateEvent("example:second_session", 2));
        await coordinator.CompleteSessionAsync(secondSession);

        string output = await File.ReadAllTextAsync(targetPath);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement[] traces = document.RootElement.GetProperty("traces").EnumerateArray().ToArray();
        Assert.Equal(2, traces.Length);
        Assert.Equal("example:first_session", traces[0].GetProperty("events").EnumerateArray().Single().GetProperty("name").GetString());
        Assert.Equal("example:second_session", traces[1].GetProperty("events").EnumerateArray().Single().GetProperty("name").GetString());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0007")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0008")]
    [Trait("CoverageType", "Positive")]
    public async Task ContainedCborFileSink_AggregatesMultipleSessionsThroughOneSerializedWriter()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture-contained-shared.qlog.cbor");

        await using QlogCaptureCoordinator coordinator = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(new QlogCborFileCaptureSink(targetPath));

        QlogCaptureSession firstSession = CreateSession("contained-cbor-first-session");
        coordinator.Capture(firstSession, CreateEvent("example:first_cbor_session", 31));
        await coordinator.CompleteSessionAsync(firstSession);

        QlogCaptureSession secondSession = CreateSession("contained-cbor-second-session");
        coordinator.Capture(secondSession, CreateEvent("example:second_cbor_session", 32));
        await coordinator.CompleteSessionAsync(secondSession);

        byte[] payload = await File.ReadAllBytesAsync(targetPath);
        CborReader reader = new(payload, CborConformanceMode.Strict);
        var rootMap = RequireMap(ReadCborValue(reader));
        var traces = RequireList(rootMap["traces"]);

        Assert.Equal(2, traces.Count);
        Assert.Equal("example:first_cbor_session", RequireString(RequireMap(RequireList(RequireMap(traces[0])["events"]).Single())["name"]));
        Assert.Equal("example:second_cbor_session", RequireString(RequireMap(RequireList(RequireMap(traces[1])["events"]).Single())["name"]));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0007")]
    [Trait("CoverageType", "Positive")]
    public async Task ContainedSeekableStreamSink_AggregatesMultipleSessions()
    {
        await using QlogCaptureCoordinator coordinator = new();
        await using MemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(
            new QlogStreamCaptureSink(stream, format: QlogCaptureSinkFormat.ContainedJson));

        QlogCaptureSession firstSession = CreateSession("contained-stream-first");
        coordinator.Capture(firstSession, CreateEvent("example:first_stream_session", 3));
        await coordinator.CompleteSessionAsync(firstSession);

        QlogCaptureSession secondSession = CreateSession("contained-stream-second");
        coordinator.Capture(secondSession, CreateEvent("example:second_stream_session", 4));
        await coordinator.CompleteSessionAsync(secondSession);

        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        JsonElement[] traces = document.RootElement.GetProperty("traces").EnumerateArray().ToArray();
        Assert.Equal(2, traces.Length);
        Assert.Equal("example:first_stream_session", traces[0].GetProperty("events").EnumerateArray().Single().GetProperty("name").GetString());
        Assert.Equal("example:second_stream_session", traces[1].GetProperty("events").EnumerateArray().Single().GetProperty("name").GetString());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0009")]
    [Trait("CoverageType", "Edge")]
    public async Task ContainedSeekableJsonStreamSink_PreservesFirstConflictValuesAndBackfillsMissingMetadata()
    {
        await using QlogCaptureCoordinator coordinator = new();
        await using MemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(
            new QlogStreamCaptureSink(stream, format: QlogCaptureSinkFormat.ContainedJson));

        Dictionary<string, QlogValue> firstFileExtensions = new(StringComparer.Ordinal)
        {
            ["shared_file_extension"] = QlogValue.FromString("first"),
            ["first_only_extension"] = QlogValue.FromString("kept"),
        };
        QlogCaptureSession firstSession = CreateSession(
            "contained-json-metadata-first",
            fileTitle: "first-title",
            fileExtensionData: firstFileExtensions);
        coordinator.Capture(firstSession, CreateEvent("example:first_metadata_session", 51));
        await coordinator.CompleteSessionAsync(firstSession);

        Dictionary<string, QlogValue> secondFileExtensions = new(StringComparer.Ordinal)
        {
            ["shared_file_extension"] = QlogValue.FromString("second"),
            ["second_only_extension"] = QlogValue.FromString("backfilled"),
        };
        QlogCaptureSession secondSession = CreateSession(
            "contained-json-metadata-second",
            fileTitle: "second-title",
            fileDescription: "second-description",
            fileExtensionData: secondFileExtensions);
        coordinator.Capture(secondSession, CreateEvent("example:second_metadata_session", 52));
        await coordinator.CompleteSessionAsync(secondSession);

        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        JsonElement rootElement = document.RootElement;

        Assert.Equal("first-title", rootElement.GetProperty("title").GetString());
        Assert.Equal("second-description", rootElement.GetProperty("description").GetString());
        Assert.Equal("first", rootElement.GetProperty("shared_file_extension").GetString());
        Assert.Equal("kept", rootElement.GetProperty("first_only_extension").GetString());
        Assert.Equal("backfilled", rootElement.GetProperty("second_only_extension").GetString());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0004")]
    [Trait("CoverageType", "Negative")]
    public async Task SinkFailures_DoNotPreventOtherObserversFromCompleting()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("isolated-sinks");

        MemoryStream failedStream = new();
        failedStream.Dispose();
        await using QlogCaptureSubscription failingSubscription = coordinator.RegisterSessionObserver(session, new QlogStreamCaptureSink(failedStream));

        await using MemoryStream goodStream = new();
        await using QlogCaptureSubscription goodSubscription = coordinator.RegisterSessionObserver(session, new QlogStreamCaptureSink(goodStream));

        coordinator.Capture(session, CreateEvent("example:isolation", 5));

        await Assert.ThrowsAnyAsync<Exception>(() => coordinator.CompleteSessionAsync(session).AsTask());

        string output = Encoding.UTF8.GetString(goodStream.ToArray());
        Assert.Contains("\"name\":\"example:isolation\"", output, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0007")]
    [Trait("CoverageType", "Negative")]
    public async Task ContainedNonSeekableStreamSink_RejectsMoreThanOneSession()
    {
        await using QlogCaptureCoordinator coordinator = new();
        await using NonSeekableMemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(
            new QlogStreamCaptureSink(stream, format: QlogCaptureSinkFormat.ContainedJson));

        QlogCaptureSession firstSession = CreateSession("contained-first-session");
        coordinator.Capture(firstSession, CreateEvent("example:first_session", 1));
        await coordinator.CompleteSessionAsync(firstSession);

        QlogCaptureSession secondSession = CreateSession("contained-second-session");
        coordinator.Capture(secondSession, CreateEvent("example:second_session", 2));
        await Assert.ThrowsAnyAsync<Exception>(() => coordinator.CompleteSessionAsync(secondSession).AsTask());

        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        JsonElement trace = Assert.Single(document.RootElement.GetProperty("traces").EnumerateArray());
        JsonElement capturedEvent = Assert.Single(trace.GetProperty("events").EnumerateArray());
        Assert.Equal("example:first_session", capturedEvent.GetProperty("name").GetString());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0007")]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0008")]
    [Trait("CoverageType", "Negative")]
    public async Task ContainedCborNonSeekableStreamSink_RejectsMoreThanOneSession()
    {
        await using QlogCaptureCoordinator coordinator = new();
        await using NonSeekableMemoryStream stream = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(new QlogCborStreamCaptureSink(stream));

        QlogCaptureSession firstSession = CreateSession("contained-cbor-first-session");
        coordinator.Capture(firstSession, CreateEvent("example:first_cbor_session", 41));
        await coordinator.CompleteSessionAsync(firstSession);

        QlogCaptureSession secondSession = CreateSession("contained-cbor-second-session");
        coordinator.Capture(secondSession, CreateEvent("example:second_cbor_session", 42));
        await Assert.ThrowsAnyAsync<Exception>(() => coordinator.CompleteSessionAsync(secondSession).AsTask());

        CborReader reader = new(stream.ToArray(), CborConformanceMode.Strict);
        var rootMap = RequireMap(ReadCborValue(reader));
        var trace = RequireMap(RequireList(rootMap["traces"]).Single());
        var capturedEvent = RequireMap(RequireList(trace["events"]).Single());

        Assert.Equal("example:first_cbor_session", RequireString(capturedEvent["name"]));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0009")]
    [Trait("CoverageType", "Edge")]
    public async Task ContainedCborFileSink_PreservesFirstConflictValuesAndBackfillsMissingMetadata()
    {
        string root = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        string targetPath = Path.Combine(root, "nested", "capture-contained-shared.qlog.cbor");

        await using QlogCaptureCoordinator coordinator = new();
        await using QlogCaptureSubscription subscription = coordinator.RegisterProcessObserver(new QlogCborFileCaptureSink(targetPath));

        Dictionary<string, QlogValue> firstFileExtensions = new(StringComparer.Ordinal)
        {
            ["shared_file_extension"] = QlogValue.FromString("first"),
            ["first_only_extension"] = QlogValue.FromString("kept"),
        };
        QlogCaptureSession firstSession = CreateSession(
            "contained-cbor-metadata-first",
            fileTitle: "first-title",
            fileExtensionData: firstFileExtensions);
        coordinator.Capture(firstSession, CreateEvent("example:first_cbor_metadata_session", 61));
        await coordinator.CompleteSessionAsync(firstSession);

        Dictionary<string, QlogValue> secondFileExtensions = new(StringComparer.Ordinal)
        {
            ["shared_file_extension"] = QlogValue.FromString("second"),
            ["second_only_extension"] = QlogValue.FromString("backfilled"),
        };
        QlogCaptureSession secondSession = CreateSession(
            "contained-cbor-metadata-second",
            fileTitle: "second-title",
            fileDescription: "second-description",
            fileExtensionData: secondFileExtensions);
        coordinator.Capture(secondSession, CreateEvent("example:second_cbor_metadata_session", 62));
        await coordinator.CompleteSessionAsync(secondSession);

        byte[] payload = await File.ReadAllBytesAsync(targetPath);
        CborReader reader = new(payload, CborConformanceMode.Strict);
        var rootMap = RequireMap(ReadCborValue(reader));

        Assert.Equal("first-title", RequireString(rootMap["title"]));
        Assert.Equal("second-description", RequireString(rootMap["description"]));
        Assert.Equal("first", RequireString(rootMap["shared_file_extension"]));
        Assert.Equal("kept", RequireString(rootMap["first_only_extension"]));
        Assert.Equal("backfilled", RequireString(rootMap["second_only_extension"]));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0001")]
    [Trait("CoverageType", "Negative")]
    public async Task FileSink_ReportsInvalidTargetPaths()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), "incursa-qlog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);

        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("invalid-file-path");
        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, new QlogFileCaptureSink(directoryPath));

        coordinator.Capture(session, CreateEvent("example:invalid_file", 3));

        await Assert.ThrowsAnyAsync<Exception>(() => coordinator.CompleteSessionAsync(session).AsTask());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-SINKS-S1-0002")]
    [Trait("CoverageType", "Negative")]
    public async Task StreamSink_ReportsClosedStreams()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("closed-stream");

        MemoryStream stream = new();
        stream.Dispose();

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, new QlogStreamCaptureSink(stream));
        coordinator.Capture(session, CreateEvent("example:closed_stream", 8));

        await Assert.ThrowsAnyAsync<Exception>(() => coordinator.CompleteSessionAsync(session).AsTask());
    }

    private static QlogCaptureSession CreateSession(
        string sessionId,
        string? fileTitle = "sink-file",
        string? fileDescription = null,
        IEnumerable<KeyValuePair<string, QlogValue>>? fileExtensionData = null)
    {
        QlogTrace trace = new()
        {
            Title = "sink-trace",
            CommonFields = new QlogCommonFields
            {
                GroupId = "sink-group",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
            VantagePoint = new QlogVantagePoint
            {
                Type = QlogKnownValues.ServerVantagePoint,
            },
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));

        return new QlogCaptureSession(
            sessionId,
            trace,
            fileTitle: fileTitle,
            fileDescription: fileDescription,
            fileExtensionData: fileExtensionData);
    }

    private static QlogEvent CreateEvent(string name, int packetSize)
    {
        QlogEvent qlogEvent = new()
        {
            Time = packetSize,
            Name = name,
            GroupId = "sink-group",
        };
        qlogEvent.Data["packet_size"] = QlogValue.FromNumber(packetSize);
        return qlogEvent;
    }

    private static object? ReadCborValue(CborReader reader)
    {
        return reader.PeekState() switch
        {
            CborReaderState.StartMap => ReadCborMap(reader),
            CborReaderState.StartArray => ReadCborArray(reader),
            CborReaderState.TextString => reader.ReadTextString(),
            CborReaderState.Boolean => reader.ReadBoolean(),
            CborReaderState.Null => ReadCborNull(reader),
            CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger => reader.ReadInt64(),
            CborReaderState.SinglePrecisionFloat => (double)reader.ReadSingle(),
            CborReaderState.DoublePrecisionFloat => reader.ReadDouble(),
            CborReaderState.HalfPrecisionFloat => (double)reader.ReadHalf(),
            _ => throw new InvalidOperationException($"Unsupported reader state '{reader.PeekState()}'."),
        };
    }

    private static Dictionary<string, object?> ReadCborMap(CborReader reader)
    {
        int? length = reader.ReadStartMap();
        Dictionary<string, object?> values = new(StringComparer.Ordinal);

        if (length.HasValue)
        {
            for (int index = 0; index < length.Value; index++)
            {
                values.Add(reader.ReadTextString(), ReadCborValue(reader));
            }
        }
        else
        {
            while (reader.PeekState() is not CborReaderState.EndMap)
            {
                values.Add(reader.ReadTextString(), ReadCborValue(reader));
            }
        }

        reader.ReadEndMap();
        return values;
    }

    private static List<object?> ReadCborArray(CborReader reader)
    {
        int? length = reader.ReadStartArray();
        List<object?> values = new();

        if (length.HasValue)
        {
            values.Capacity = length.Value;
            for (int index = 0; index < length.Value; index++)
            {
                values.Add(ReadCborValue(reader));
            }
        }
        else
        {
            while (reader.PeekState() is not CborReaderState.EndArray)
            {
                values.Add(ReadCborValue(reader));
            }
        }

        reader.ReadEndArray();
        return values;
    }

    private static object? ReadCborNull(CborReader reader)
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

    private sealed class NonSeekableMemoryStream : MemoryStream
    {
        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
