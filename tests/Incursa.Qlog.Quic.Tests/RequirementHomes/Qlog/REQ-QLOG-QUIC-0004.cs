using System.Formats.Cbor;
using System.Text.Json;
using Incursa.Qlog;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Quic.Tests.Fixtures;
using Xunit;

namespace Incursa.Qlog.Quic.Tests;

public sealed class REQ_QLOG_QUIC_0004
{
    private const string FixtureRoot = "Fixtures";
    private const string ArtifactRoot = "Artifacts";

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S4-0001")]
    [Trait("CoverageType", "Positive")]
    public void LoadContainedJsonFixture_HydratesFileTraceErrorVantagePointAndLifecycleObjects()
    {
        QlogFile file = QlogFixtureLoader.LoadQlog(FixtureRoot, ArtifactRoot, "captured-quic-contained.qlog.json");

        Assert.Equal(QlogKnownValues.ContainedFileSchemaUri, file.FileSchema);
        Assert.Equal(QlogKnownValues.ContainedJsonSerializationFormat, file.SerializationFormat);
        Assert.Equal("Captured QUIC fixture", file.Title);
        Assert.Equal(QlogValue.FromString("kept"), file.ExtensionData["fixture_file_extension"]);
        Assert.Equal(2, file.Traces.Count);

        QlogTrace trace = Assert.IsType<QlogTrace>(file.Traces[0]);
        Assert.Equal("client migration and stream fixture", trace.Title);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(trace.EventSchemas));
        Assert.Equal(QlogValue.FromString("kept"), trace.ExtensionData["trace_extension"]);

        Assert.NotNull(trace.CommonFields);
        Assert.Equal("tuple-1", trace.CommonFields.Tuple);
        Assert.Equal(QlogKnownValues.RelativeToEpochTimeFormat, trace.CommonFields.TimeFormat);
        Assert.Equal("connection-1", trace.CommonFields.GroupId);
        Assert.Equal(QlogValue.FromString("kept"), trace.CommonFields.ExtensionData["common_extension"]);
        Assert.NotNull(trace.CommonFields.ReferenceTime);
        Assert.Equal(QlogKnownValues.SystemClockType, trace.CommonFields.ReferenceTime.ClockType);
        Assert.Equal(QlogValue.FromString("kept"), trace.CommonFields.ReferenceTime.ExtensionData["reference_extension"]);

        QlogVantagePoint vantagePoint = Assert.IsType<QlogVantagePoint>(trace.VantagePoint);
        Assert.Equal("neqo-client", vantagePoint.Name);
        Assert.Equal(QlogKnownValues.ClientVantagePoint, vantagePoint.Type);
        Assert.Equal("downstream", vantagePoint.Flow);
        Assert.Equal(QlogValue.FromString("kept"), vantagePoint.ExtensionData["vantage_extension"]);

        QlogEvent connectionStarted = RequireEvent(trace, QlogQuicKnownValues.ConnectionStartedEventName);
        Assert.Equal(0, connectionStarted.Time);
        Assert.Equal("tuple-1", connectionStarted.Tuple);
        Assert.Equal(QlogKnownValues.RelativeToEpochTimeFormat, connectionStarted.TimeFormat);
        Assert.Equal("connection-1", connectionStarted.GroupId);
        Assert.NotNull(connectionStarted.SystemInfo);
        Assert.Equal(QlogValue.FromString("interop"), connectionStarted.SystemInfo["runner"]);
        Assert.Equal(QlogValue.FromString("kept"), connectionStarted.ExtensionData["event_extension"]);
        QlogFixtureAssertions.AssertJsonEquivalent(
            """{"ip_v6":"fd00:cafe:cafe::100","port_v6":41714,"connection_ids":["2021222324252627"]}""",
            connectionStarted.Data["local"]);
        Assert.Equal(QlogValue.FromString("connection-started"), connectionStarted.Data["event_marker"]);

        QlogEvent connectionStateUpdated = RequireEvent(trace, QlogQuicKnownValues.ConnectionStateUpdatedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.ConnectionStateHandshakeStarted), connectionStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.ConnectionStateHandshakeComplete), connectionStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString("kept"), connectionStateUpdated.Data["state_extension"]);

        QlogEvent connectionClosed = RequireEvent(trace, QlogQuicKnownValues.ConnectionClosedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.LocalInitiator), connectionClosed.Data["initiator"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.CloseTriggerIdleTimeout), connectionClosed.Data["trigger"]);
        Assert.Equal(QlogValue.FromString("kept"), connectionClosed.Data["closed_extension"]);

        QlogTraceError traceError = Assert.IsType<QlogTraceError>(file.Traces[1]);
        Assert.Equal("secondary capture segment omitted key material", traceError.ErrorDescription);
        Assert.Equal("qlog://fixtures/captured-quic-contained/trace-error", traceError.Uri);
        Assert.NotNull(traceError.VantagePoint);
        Assert.Equal(QlogKnownValues.ServerVantagePoint, traceError.VantagePoint.Type);
        Assert.Equal(QlogValue.FromString("kept"), traceError.ExtensionData["trace_error_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P2-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P3-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S6-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S7-0001")]
    [Trait("CoverageType", "Positive")]
    public void LoadContainedJsonFixture_HydratesPacketDatagramStreamMigrationKeyAndRecoveryObjects()
    {
        QlogTrace trace = LoadContainedFixtureTrace();

        QlogEvent packetSent = RequireEvent(trace, QlogQuicKnownValues.PacketSentEventName);
        QlogFixtureAssertions.AssertJsonEquivalent("""{"packet_type":"1RTT","packet_number":103,"key_phase":0}""", packetSent.Data["header"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""[{"frame_type":"stream","stream_id":0,"offset":0,"length":37}]""", packetSent.Data["frames"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""{"length":1250,"payload_length":1200,"data":"aa55"}""", packetSent.Data["raw"]);
        Assert.Equal(QlogValue.FromNumber(9), packetSent.Data["datagram_id"]);
        Assert.Equal(QlogValue.FromBoolean(false), packetSent.Data["is_mtu_probe_packet"]);
        Assert.Equal(QlogValue.FromString("kept"), packetSent.Data["packet_sent_extension"]);

        QlogEvent udpDatagramsReceived = RequireEvent(trace, QlogQuicKnownValues.UdpDatagramsReceivedEventName);
        Assert.Equal(QlogValue.FromNumber(1), udpDatagramsReceived.Data["count"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""[{"length":1250,"payload_length":1200}]""", udpDatagramsReceived.Data["raw"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""["Not-ECT"]""", udpDatagramsReceived.Data["ecn"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""[9]""", udpDatagramsReceived.Data["datagram_ids"]);
        Assert.Equal(QlogValue.FromString("kept"), udpDatagramsReceived.Data["udp_received_extension"]);

        QlogEvent streamStateUpdated = RequireEvent(trace, QlogQuicKnownValues.StreamStateUpdatedEventName);
        Assert.Equal(QlogValue.FromNumber(0), streamStateUpdated.Data["stream_id"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.StreamTypeBidirectional), streamStateUpdated.Data["stream_type"]);
        Assert.Equal(QlogValue.FromString("half_closed_remote"), streamStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString("kept"), streamStateUpdated.Data["stream_state_extension"]);

        QlogEvent streamDataMoved = RequireEvent(trace, QlogQuicKnownValues.StreamDataMovedEventName);
        Assert.Equal(QlogValue.FromNumber(37), streamDataMoved.Data["offset"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationTransport), streamDataMoved.Data["from"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationApplication), streamDataMoved.Data["to"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataMovedAdditionalInfoFinSet), streamDataMoved.Data["additional_info"]);
        Assert.Equal(QlogValue.FromString("kept"), streamDataMoved.Data["stream_data_extension"]);

        QlogEvent datagramDataMoved = RequireEvent(trace, QlogQuicKnownValues.DatagramDataMovedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationNetwork), datagramDataMoved.Data["from"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationApplication), datagramDataMoved.Data["to"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""{"length":37}""", datagramDataMoved.Data["raw"]);
        Assert.Equal(QlogValue.FromString("kept"), datagramDataMoved.Data["datagram_data_extension"]);

        QlogEvent migrationStateUpdated = RequireEvent(trace, QlogQuicKnownValues.MigrationStateUpdatedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.MigrationStateProbingStarted), migrationStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.MigrationStateProbingSuccessful), migrationStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString("preferred-v6"), migrationStateUpdated.Data["tuple_id"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""{"ip_v6":"fd00:cafe:cafe:100::110","port_v6":443}""", migrationStateUpdated.Data["tuple_local"]);
        Assert.Equal(QlogValue.FromString("kept"), migrationStateUpdated.Data["migration_extension"]);

        QlogEvent keyUpdated = RequireEvent(trace, QlogQuicKnownValues.KeyUpdatedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyTypeServerOneRttSecret), keyUpdated.Data["key_type"]);
        Assert.Equal(QlogValue.FromNumber(1), keyUpdated.Data["key_phase"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyLifecycleTriggerLocalUpdate), keyUpdated.Data["trigger"]);

        QlogEvent recoveryMetricsUpdated = RequireEvent(trace, QlogQuicKnownValues.RecoveryMetricsUpdatedEventName);
        Assert.Equal(QlogValue.FromNumber(12.25), recoveryMetricsUpdated.Data["smoothed_rtt"]);
        Assert.Equal(QlogValue.FromNumber(12000), recoveryMetricsUpdated.Data["bytes_in_flight"]);
        Assert.Equal(QlogValue.FromString("kept"), recoveryMetricsUpdated.Data["recovery_metrics_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S4-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P2-0001")]
    [Trait("CoverageType", "Positive")]
    public void LoadSequentialJsonTextSequenceFixture_HydratesHeaderAndEventRecordsThroughAutoDetection()
    {
        QlogFile file = QlogFixtureLoader.LoadQlog(FixtureRoot, ArtifactRoot, "captured-quic-sequential.sqlog");

        Assert.Equal(QlogKnownValues.SequentialFileSchemaUri, file.FileSchema);
        Assert.Equal(QlogKnownValues.SequentialJsonTextSequencesSerializationFormat, file.SerializationFormat);
        Assert.Equal(QlogValue.FromString("kept"), file.ExtensionData["file_extension"]);

        QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(file.Traces));
        Assert.Equal("sequential client fixture", trace.Title);
        Assert.Equal(QlogValue.FromString("kept"), trace.ExtensionData["trace_extension"]);
        Assert.NotNull(trace.CommonFields);
        Assert.Equal("tuple-seq", trace.CommonFields.Tuple);
        Assert.NotNull(trace.VantagePoint);
        Assert.Equal(QlogValue.FromString("kept"), trace.VantagePoint.ExtensionData["vantage_extension"]);
        Assert.Equal(3, trace.Events.Count);

        QlogEvent packetReceived = RequireEvent(trace, QlogQuicKnownValues.PacketReceivedEventName);
        QlogFixtureAssertions.AssertJsonEquivalent("""{"packet_type":"initial","packet_number":1}""", packetReceived.Data["header"]);
        QlogFixtureAssertions.AssertJsonEquivalent("""["00000001"]""", packetReceived.Data["supported_versions"]);
        Assert.Equal(QlogValue.FromString("kept"), packetReceived.Data["packet_received_extension"]);

        QlogEvent connectionClosed = RequireEvent(trace, QlogQuicKnownValues.ConnectionClosedEventName);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.RemoteInitiator), connectionClosed.Data["initiator"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.CloseTriggerApplication), connectionClosed.Data["trigger"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0005")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S6-0001")]
    [Trait("CoverageType", "Positive")]
    public void LoadKnownContainedCborFixture_UsesExplicitCborImportAndHydratesQuicObjects()
    {
        string fixturePath = QlogFixtureLoader.GetFixturePath(FixtureRoot, ArtifactRoot, "generated-known-contained-cbor.qlog.cbor");
        WriteKnownContainedCborFixture(fixturePath);

        try
        {
            QlogFile file = QlogFixtureLoader.LoadContainedCborQlog(FixtureRoot, ArtifactRoot, "generated-known-contained-cbor.qlog.cbor");

            Assert.Equal(QlogKnownValues.ContainedFileSchemaUri, file.FileSchema);
            Assert.Equal("application/cbor", file.SerializationFormat);
            Assert.Equal(QlogValue.FromString("kept"), file.ExtensionData["cbor_file_extension"]);

            QlogTrace trace = Assert.IsType<QlogTrace>(Assert.Single(file.Traces));
            Assert.NotNull(trace.VantagePoint);
            Assert.Equal(QlogKnownValues.ServerVantagePoint, trace.VantagePoint.Type);
            Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(trace.EventSchemas));

            QlogEvent keyDiscarded = RequireEvent(trace, QlogQuicKnownValues.KeyDiscardedEventName);
            Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyTypeServerHandshakeSecret), keyDiscarded.Data["key_type"]);
            Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyLifecycleTriggerTls), keyDiscarded.Data["trigger"]);
            Assert.Equal(QlogValue.FromString("kept"), keyDiscarded.Data["key_discarded_extension"]);
        }
        finally
        {
            File.Delete(fixturePath);
        }
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0003")]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("CoverageType", "Negative")]
    public void LoadQlogFixture_RejectsMalformedOrUnsupportedImportInput()
    {
        Assert.ThrowsAny<JsonException>(() => QlogFixtureLoader.LoadQlog(FixtureRoot, ArtifactRoot, "malformed-contained.qlog.json"));

        InvalidOperationException unsupported = Assert.Throws<InvalidOperationException>(
            () => QlogFixtureLoader.LoadQlog(FixtureRoot, ArtifactRoot, "unsupported-input.qlog"));
        Assert.Contains("Unsupported qlog input format", unsupported.Message, StringComparison.Ordinal);
    }

    private static QlogTrace LoadContainedFixtureTrace()
    {
        QlogFile file = QlogFixtureLoader.LoadQlog(FixtureRoot, ArtifactRoot, "captured-quic-contained.qlog.json");
        return Assert.IsType<QlogTrace>(file.Traces[0]);
    }

    private static QlogEvent RequireEvent(QlogTrace trace, string eventName)
    {
        return Assert.Single(trace.Events, qlogEvent => qlogEvent.Name == eventName);
    }

    private static void WriteKnownContainedCborFixture(string fixturePath)
    {
        CborWriter writer = new(CborConformanceMode.Strict);

        writer.WriteStartMap(5);
        writer.WriteTextString("file_schema");
        writer.WriteTextString(QlogKnownValues.ContainedFileSchemaUri.OriginalString);
        writer.WriteTextString("serialization_format");
        writer.WriteTextString("application/cbor");
        writer.WriteTextString("title");
        writer.WriteTextString("Known contained CBOR QUIC fixture");
        writer.WriteTextString("traces");
        writer.WriteStartArray(1);
        WriteCborTrace(writer);
        writer.WriteEndArray();
        writer.WriteTextString("cbor_file_extension");
        writer.WriteTextString("kept");
        writer.WriteEndMap();

        File.WriteAllBytes(fixturePath, writer.Encode());
    }

    private static void WriteCborTrace(CborWriter writer)
    {
        writer.WriteStartMap(5);
        writer.WriteTextString("title");
        writer.WriteTextString("server key lifecycle fixture");
        writer.WriteTextString("vantage_point");
        writer.WriteStartMap(2);
        writer.WriteTextString("name");
        writer.WriteTextString("incursa-server");
        writer.WriteTextString("type");
        writer.WriteTextString(QlogKnownValues.ServerVantagePoint);
        writer.WriteEndMap();
        writer.WriteTextString("event_schemas");
        writer.WriteStartArray(1);
        writer.WriteTextString(QlogQuicKnownValues.DraftEventSchemaUri.OriginalString);
        writer.WriteEndArray();
        writer.WriteTextString("events");
        writer.WriteStartArray(1);
        WriteCborKeyDiscardedEvent(writer);
        writer.WriteEndArray();
        writer.WriteTextString("trace_extension");
        writer.WriteTextString("kept");
        writer.WriteEndMap();
    }

    private static void WriteCborKeyDiscardedEvent(CborWriter writer)
    {
        writer.WriteStartMap(4);
        writer.WriteTextString("time");
        writer.WriteDouble(1);
        writer.WriteTextString("name");
        writer.WriteTextString(QlogQuicKnownValues.KeyDiscardedEventName);
        writer.WriteTextString("data");
        writer.WriteStartMap(4);
        writer.WriteTextString("key_type");
        writer.WriteTextString(QlogQuicKnownValues.KeyTypeServerHandshakeSecret);
        writer.WriteTextString("key");
        writer.WriteTextString("8899aabb");
        writer.WriteTextString("trigger");
        writer.WriteTextString(QlogQuicKnownValues.KeyLifecycleTriggerTls);
        writer.WriteTextString("key_discarded_extension");
        writer.WriteTextString("kept");
        writer.WriteEndMap();
        writer.WriteTextString("event_extension");
        writer.WriteTextString("kept");
        writer.WriteEndMap();
    }
}
