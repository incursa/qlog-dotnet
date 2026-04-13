using System.Text.Json.Nodes;
using Incursa.Qlog;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Quic.Tests;

public sealed class REQ_QLOG_QUIC_0003
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S6-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicMigrationAndKeyLifecycleEventsAndRoundTripsTheDraftShape()
    {
        QuicConnectionIdUpdated connectionIdUpdatedPayload = new()
        {
            Initiator = QlogQuicKnownValues.LocalInitiator,
            Old = "0a0b",
            New = "0c0d",
        };
        connectionIdUpdatedPayload.ExtensionData["connection_id_extension"] = QlogValue.FromString("kept");
        QlogEvent connectionIdUpdated = QlogQuicEvents.CreateConnectionIdUpdated(0, connectionIdUpdatedPayload);

        QuicTupleEndpointInfo tupleRemote = new()
        {
            IpV4 = "198.51.100.2",
            PortV4 = 443,
        };
        tupleRemote.ConnectionIds.Add("0a0b");
        tupleRemote.ExtensionData["tuple_remote_extension"] = QlogValue.FromString("kept");

        QuicTupleEndpointInfo tupleLocal = new()
        {
            IpV4 = "203.0.113.10",
            PortV4 = 443,
        };
        tupleLocal.ConnectionIds.Add("0c0d");
        tupleLocal.ExtensionData["tuple_local_extension"] = QlogValue.FromString("kept");

        QuicMigrationStateUpdated migrationStateUpdatedPayload = new()
        {
            Old = QlogQuicKnownValues.MigrationStateProbingStarted,
            New = QlogQuicKnownValues.MigrationStateComplete,
            TupleId = "tuple-7",
            TupleRemote = tupleRemote,
            TupleLocal = tupleLocal,
        };
        migrationStateUpdatedPayload.ExtensionData["migration_extension"] = QlogValue.FromString("kept");
        QlogEvent migrationStateUpdated = QlogQuicEvents.CreateMigrationStateUpdated(1, migrationStateUpdatedPayload);

        QuicKeyUpdated keyUpdatedPayload = new()
        {
            KeyType = QlogQuicKnownValues.KeyTypeClientOneRttSecret,
            Old = "00112233",
            New = "44556677",
            KeyPhase = 4,
            Trigger = QlogQuicKnownValues.KeyLifecycleTriggerRemoteUpdate,
        };
        keyUpdatedPayload.ExtensionData["key_updated_extension"] = QlogValue.FromString("kept");
        QlogEvent keyUpdated = QlogQuicEvents.CreateKeyUpdated(2, keyUpdatedPayload);

        QuicKeyDiscarded keyDiscardedPayload = new()
        {
            KeyType = QlogQuicKnownValues.KeyTypeServerHandshakeSecret,
            Key = "8899aabb",
            Trigger = QlogQuicKnownValues.KeyLifecycleTriggerTls,
        };
        keyDiscardedPayload.ExtensionData["key_discarded_extension"] = QlogValue.FromString("kept");
        QlogEvent keyDiscarded = QlogQuicEvents.CreateKeyDiscarded(3, keyDiscardedPayload);

        QlogTrace trace = CreateQuicTrace(connectionIdUpdated, migrationStateUpdated, keyUpdated, keyDiscarded);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Single(trace.EventSchemas);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, trace.EventSchemas[0]);
        Assert.All(trace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));

        Assert.Equal(QlogValue.FromString("kept"), connectionIdUpdated.Data["connection_id_extension"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.LocalInitiator), connectionIdUpdated.Data["initiator"]);
        Assert.Equal(QlogValue.FromString("0a0b"), connectionIdUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString("0c0d"), connectionIdUpdated.Data["new"]);

        Assert.Equal(QlogValue.FromString("kept"), migrationStateUpdated.Data["migration_extension"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.MigrationStateProbingStarted), migrationStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.MigrationStateComplete), migrationStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString("tuple-7"), migrationStateUpdated.Data["tuple_id"]);
        Assert.Equal(
            QlogValue.Parse("""{"ip_v4":"198.51.100.2","port_v4":443,"connection_ids":["0a0b"],"tuple_remote_extension":"kept"}"""),
            migrationStateUpdated.Data["tuple_remote"]);
        Assert.Equal(
            QlogValue.Parse("""{"ip_v4":"203.0.113.10","port_v4":443,"connection_ids":["0c0d"],"tuple_local_extension":"kept"}"""),
            migrationStateUpdated.Data["tuple_local"]);

        Assert.Equal(QlogValue.FromString("kept"), keyUpdated.Data["key_updated_extension"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyTypeClientOneRttSecret), keyUpdated.Data["key_type"]);
        Assert.Equal(QlogValue.FromString("00112233"), keyUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString("44556677"), keyUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromNumber(4), keyUpdated.Data["key_phase"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyLifecycleTriggerRemoteUpdate), keyUpdated.Data["trigger"]);

        Assert.Equal(QlogValue.FromString("kept"), keyDiscarded.Data["key_discarded_extension"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyTypeServerHandshakeSecret), keyDiscarded.Data["key_type"]);
        Assert.Equal(QlogValue.FromString("8899aabb"), keyDiscarded.Data["key"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.KeyLifecycleTriggerTls), keyDiscarded.Data["trigger"]);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);

        JsonNode? originalNode = JsonNode.Parse(json);
        JsonNode? roundTrippedNode = JsonNode.Parse(roundTrippedJson);
        Assert.True(JsonNode.DeepEquals(originalNode, roundTrippedNode));

        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces.Single());
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(parsedTrace.EventSchemas));
        Assert.All(parsedTrace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[0].Data["connection_id_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[1].Data["migration_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[2].Data["key_updated_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[3].Data["key_discarded_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S6-0001")]
    [Trait("CoverageType", "Edge")]
    public void Serialize_WritesMigrationStateUpdatedWithAnEmptyTupleId()
    {
        QuicMigrationStateUpdated payload = new()
        {
            New = QlogQuicKnownValues.MigrationStateStarted,
            TupleId = string.Empty,
        };
        payload.ExtensionData["migration_edge_extension"] = QlogValue.FromString("kept");

        QlogEvent migrationStateUpdated = QlogQuicEvents.CreateMigrationStateUpdated(0, payload);
        QlogTrace trace = CreateQuicTrace(migrationStateUpdated);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Equal(QlogValue.FromString(string.Empty), migrationStateUpdated.Data["tuple_id"]);
        Assert.Equal(QlogValue.FromString("kept"), migrationStateUpdated.Data["migration_edge_extension"]);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);
        Assert.Equal(QlogValue.FromString(string.Empty), Assert.IsType<QlogTrace>(parsed.Traces.Single()).Events.Single().Data["tuple_id"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S6-0001")]
    [Trait("CoverageType", "Negative")]
    public void CreateMigrationAndKeyLifecycleEvents_RejectsInvalidKnownValues()
    {
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateConnectionIdUpdated(0, new QuicConnectionIdUpdated()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateMigrationStateUpdated(0, new QuicMigrationStateUpdated
        {
            New = "made_up",
        }));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateKeyUpdated(0, new QuicKeyUpdated
        {
            KeyType = "mystery",
        }));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateKeyDiscarded(0, new QuicKeyDiscarded
        {
            KeyType = QlogQuicKnownValues.KeyTypeClientOneRttSecret,
            Trigger = "invalid",
        }));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S7-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicRecoveryAndPacketLossEventsAndRoundTripsTheDraftShape()
    {
        QuicRecoveryParametersSet recoveryParametersSetPayload = new()
        {
            ReorderingThreshold = 3,
            TimeThreshold = 1.125,
            TimerGranularity = 1,
            InitialRtt = 333.5,
            MaxDatagramSize = 1350,
            InitialCongestionWindow = 14720,
            MinimumCongestionWindow = 2700,
            LossReductionFactor = 0.5,
            PersistentCongestionThreshold = 3,
        };
        recoveryParametersSetPayload.ExtensionData["recovery_parameters_extension"] = QlogValue.FromString("kept");
        QlogEvent recoveryParametersSet = QlogQuicEvents.CreateRecoveryParametersSet(0, recoveryParametersSetPayload);

        QuicRecoveryMetricsUpdated recoveryMetricsUpdatedPayload = new()
        {
            MinRtt = 9.5,
            SmoothedRtt = 12.25,
            LatestRtt = 14.75,
            RttVariance = 1.5,
            PtoCount = 2,
            CongestionWindow = 24000,
            BytesInFlight = 12000,
            Ssthresh = 18000,
            PacketsInFlight = 10,
            PacingRate = 960000,
        };
        recoveryMetricsUpdatedPayload.ExtensionData["recovery_metrics_extension"] = QlogValue.FromString("kept");
        QlogEvent recoveryMetricsUpdated = QlogQuicEvents.CreateRecoveryMetricsUpdated(1, recoveryMetricsUpdatedPayload);

        QuicCongestionStateUpdated congestionStateUpdatedPayload = new()
        {
            Old = "slow_start",
            New = "congestion_avoidance",
            Trigger = "ack",
        };
        congestionStateUpdatedPayload.ExtensionData["congestion_extension"] = QlogValue.FromString("kept");
        QlogEvent congestionStateUpdated = QlogQuicEvents.CreateCongestionStateUpdated(2, congestionStateUpdatedPayload);

        QuicPacketLost packetLostPayload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeOneRtt,
                KeyPhase = 4,
                PacketNumber = 144,
            },
            IsMtuProbePacket = false,
            Trigger = QlogQuicKnownValues.PacketLostTriggerPtoExpired,
        };
        packetLostPayload.Header.ExtensionData["packet_lost_header_extension"] = QlogValue.FromString("kept");
        packetLostPayload.Frames.Add(QlogValue.Parse("""{"frame_type":"stream","stream_id":4,"offset":128,"length":32}"""));
        packetLostPayload.ExtensionData["packet_lost_extension"] = QlogValue.FromString("kept");
        QlogEvent packetLost = QlogQuicEvents.CreatePacketLost(3, packetLostPayload);

        QlogTrace trace = CreateQuicTrace(recoveryParametersSet, recoveryMetricsUpdated, congestionStateUpdated, packetLost);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Equal(QlogValue.FromNumber(3), recoveryParametersSet.Data["reordering_threshold"]);
        Assert.Equal(QlogValue.FromNumber(1.125), recoveryParametersSet.Data["time_threshold"]);
        Assert.Equal(QlogValue.FromNumber(1), recoveryParametersSet.Data["timer_granularity"]);
        Assert.Equal(QlogValue.FromNumber(333.5), recoveryParametersSet.Data["initial_rtt"]);
        Assert.Equal(QlogValue.FromNumber(1350), recoveryParametersSet.Data["max_datagram_size"]);
        Assert.Equal(QlogValue.FromNumber(14720), recoveryParametersSet.Data["initial_congestion_window"]);
        Assert.Equal(QlogValue.FromNumber(2700), recoveryParametersSet.Data["minimum_congestion_window"]);
        Assert.Equal(QlogValue.FromNumber(0.5), recoveryParametersSet.Data["loss_reduction_factor"]);
        Assert.Equal(QlogValue.FromNumber(3), recoveryParametersSet.Data["persistent_congestion_threshold"]);
        Assert.Equal(QlogValue.FromString("kept"), recoveryParametersSet.Data["recovery_parameters_extension"]);

        Assert.Equal(QlogValue.FromNumber(9.5), recoveryMetricsUpdated.Data["min_rtt"]);
        Assert.Equal(QlogValue.FromNumber(12.25), recoveryMetricsUpdated.Data["smoothed_rtt"]);
        Assert.Equal(QlogValue.FromNumber(14.75), recoveryMetricsUpdated.Data["latest_rtt"]);
        Assert.Equal(QlogValue.FromNumber(1.5), recoveryMetricsUpdated.Data["rtt_variance"]);
        Assert.Equal(QlogValue.FromNumber(2), recoveryMetricsUpdated.Data["pto_count"]);
        Assert.Equal(QlogValue.FromNumber(24000), recoveryMetricsUpdated.Data["congestion_window"]);
        Assert.Equal(QlogValue.FromNumber(12000), recoveryMetricsUpdated.Data["bytes_in_flight"]);
        Assert.Equal(QlogValue.FromNumber(18000), recoveryMetricsUpdated.Data["ssthresh"]);
        Assert.Equal(QlogValue.FromNumber(10), recoveryMetricsUpdated.Data["packets_in_flight"]);
        Assert.Equal(QlogValue.FromNumber(960000), recoveryMetricsUpdated.Data["pacing_rate"]);
        Assert.Equal(QlogValue.FromString("kept"), recoveryMetricsUpdated.Data["recovery_metrics_extension"]);

        Assert.Equal(QlogValue.FromString("slow_start"), congestionStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString("congestion_avoidance"), congestionStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString("ack"), congestionStateUpdated.Data["trigger"]);
        Assert.Equal(QlogValue.FromString("kept"), congestionStateUpdated.Data["congestion_extension"]);

        Assert.Equal(
            QlogValue.Parse("""{"packet_type":"1RTT","key_phase":4,"packet_number":144,"packet_lost_header_extension":"kept"}"""),
            packetLost.Data["header"]);
        Assert.Equal(
            QlogValue.Parse("""[{"frame_type":"stream","stream_id":4,"offset":128,"length":32}]"""),
            packetLost.Data["frames"]);
        Assert.Equal(QlogValue.FromBoolean(false), packetLost.Data["is_mtu_probe_packet"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.PacketLostTriggerPtoExpired), packetLost.Data["trigger"]);
        Assert.Equal(QlogValue.FromString("kept"), packetLost.Data["packet_lost_extension"]);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);

        JsonNode? originalNode = JsonNode.Parse(json);
        JsonNode? roundTrippedNode = JsonNode.Parse(roundTrippedJson);
        Assert.True(JsonNode.DeepEquals(originalNode, roundTrippedNode));

        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces.Single());
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(parsedTrace.EventSchemas));
        Assert.All(parsedTrace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[0].Data["recovery_parameters_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[1].Data["recovery_metrics_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[2].Data["congestion_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[3].Data["packet_lost_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S7-0001")]
    [Trait("CoverageType", "Edge")]
    public void Serialize_WritesMinimalPacketLostShape()
    {
        QuicPacketLost payload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeHandshake,
                PacketNumber = 88,
            },
        };
        payload.ExtensionData["packet_lost_edge_extension"] = QlogValue.FromString("kept");

        QlogEvent packetLost = QlogQuicEvents.CreatePacketLost(0, payload);
        QlogTrace trace = CreateQuicTrace(packetLost);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Equal(
            QlogValue.Parse("""{"packet_type":"handshake","packet_number":88}"""),
            packetLost.Data["header"]);
        Assert.Equal(QlogValue.FromString("kept"), packetLost.Data["packet_lost_edge_extension"]);
        Assert.False(packetLost.Data.ContainsKey("frames"));
        Assert.False(packetLost.Data.ContainsKey("is_mtu_probe_packet"));
        Assert.False(packetLost.Data.ContainsKey("trigger"));

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);
        Assert.Equal(QlogValue.FromString("kept"), Assert.IsType<QlogTrace>(parsed.Traces.Single()).Events.Single().Data["packet_lost_edge_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S7-0001")]
    [Trait("CoverageType", "Negative")]
    public void CreateStateAndRecoveryEvents_RejectsMissingRequiredTypedFields()
    {
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateRecoveryParametersSet(0, new QuicRecoveryParametersSet()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateRecoveryMetricsUpdated(0, new QuicRecoveryMetricsUpdated()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateCongestionStateUpdated(0, new QuicCongestionStateUpdated()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreatePacketLost(0, new QuicPacketLost
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeOneRtt,
            },
            Trigger = "not_a_valid_trigger",
        }));
    }

    private static QlogTrace CreateQuicTrace(params QlogEvent[] events)
    {
        QlogTrace trace = new();
        QlogQuicEvents.RegisterDraftSchema(trace);

        foreach (QlogEvent qlogEvent in events)
        {
            trace.Events.Add(qlogEvent);
        }

        return trace;
    }
}
