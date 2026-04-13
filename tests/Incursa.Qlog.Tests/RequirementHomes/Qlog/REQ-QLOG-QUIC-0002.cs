using System.Text.Json.Nodes;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_QUIC_0002
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P2-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicPacketAndDatagramActivityEventsAndRoundTripsTheDraftShape()
    {
        QuicPacketSent packetSentPayload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeInitial,
                PacketNumberLength = 2,
                PacketNumber = 17,
                Token = new QuicToken
                {
                    Type = QlogQuicKnownValues.TokenTypeRetry,
                    Raw = new QuicRawInfo
                    {
                        Length = 16,
                        Data = "00112233445566778899aabbccddeeff",
                    },
                },
                Length = 1250,
                Version = "00000001",
                Scil = 0,
                Dcil = 4,
                Scid = "0a0b",
                Dcid = "0c0d",
            },
            Raw = new QuicRawInfo
            {
                Length = 1250,
                PayloadLength = 1200,
                Data = "aa55",
            },
            DatagramId = 7,
            IsMtuProbePacket = true,
        };
        packetSentPayload.Header.ExtensionData["header_extension"] = QlogValue.FromString("kept");
        packetSentPayload.Header.Token!.Details["token_source"] = QlogValue.FromString("server");
        packetSentPayload.Header.Token.ExtensionData["token_extension"] = QlogValue.FromString("kept");
        packetSentPayload.Frames.Add(
            QlogValue.Parse("""{"frame_type":"crypto","offset":0,"raw":{"length":20,"payload_length":4,"data":"00112233"}}"""));
        packetSentPayload.ExtensionData["packet_sent_extension"] = QlogValue.FromString("kept");
        QlogEvent packetSent = QlogQuicEvents.CreatePacketSent(0, packetSentPayload);

        QuicPacketReceived packetReceivedPayload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeVersionNegotiation,
            },
            Raw = new QuicRawInfo
            {
                Length = 1250,
                PayloadLength = 1200,
                Data = "bada55",
            },
            DatagramId = 8,
        };
        packetReceivedPayload.Header.ExtensionData["header_extension"] = QlogValue.FromString("kept");
        packetReceivedPayload.SupportedVersions.Add("00000001");
        packetReceivedPayload.SupportedVersions.Add("00000002");
        packetReceivedPayload.ExtensionData["packet_received_extension"] = QlogValue.FromString("kept");
        QlogEvent packetReceived = QlogQuicEvents.CreatePacketReceived(1, packetReceivedPayload);

        QuicPacketDropped packetDroppedPayload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeUnknown,
                PacketTypeBytes = 99,
            },
            Raw = new QuicRawInfo
            {
                Length = 64,
                PayloadLength = 64,
            },
            DatagramId = 9,
            Trigger = QlogQuicKnownValues.PacketDroppedTriggerInvalid,
        };
        packetDroppedPayload.Header.ExtensionData["header_extension"] = QlogValue.FromString("kept");
        packetDroppedPayload.Details["reason"] = QlogValue.FromString("invalid_crc");
        packetDroppedPayload.Details["retry_count"] = QlogValue.FromNumber(2);
        packetDroppedPayload.ExtensionData["packet_dropped_extension"] = QlogValue.FromString("kept");
        QlogEvent packetDropped = QlogQuicEvents.CreatePacketDropped(2, packetDroppedPayload);

        QuicPacketBuffered packetBufferedPayload = new()
        {
            Header = new QuicPacketHeader
            {
                PacketType = QlogQuicKnownValues.PacketTypeHandshake,
                PacketNumberLength = 2,
                PacketNumber = 5,
            },
            Raw = new QuicRawInfo
            {
                Length = 80,
            },
            DatagramId = 10,
            Trigger = QlogQuicKnownValues.PacketBufferedTriggerBackpressure,
        };
        packetBufferedPayload.Header.ExtensionData["header_extension"] = QlogValue.FromString("kept");
        packetBufferedPayload.ExtensionData["packet_buffered_extension"] = QlogValue.FromString("kept");
        QlogEvent packetBuffered = QlogQuicEvents.CreatePacketBuffered(3, packetBufferedPayload);

        QuicPacketsAcked packetsAckedPayload = new()
        {
            PacketNumberSpace = QlogQuicKnownValues.PacketNumberSpaceApplicationData,
        };
        packetsAckedPayload.PacketNumbers.Add(17);
        packetsAckedPayload.PacketNumbers.Add(18);
        packetsAckedPayload.PacketNumbers.Add(19);
        packetsAckedPayload.ExtensionData["packets_acked_extension"] = QlogValue.FromString("kept");
        QlogEvent packetsAcked = QlogQuicEvents.CreatePacketsAcked(4, packetsAckedPayload);

        QuicUdpDatagramsSent udpDatagramsSentPayload = new()
        {
            Count = 2,
        };
        udpDatagramsSentPayload.Raw.Add(new QuicRawInfo
        {
            Length = 1250,
            PayloadLength = 1200,
        });
        udpDatagramsSentPayload.Raw.Add(new QuicRawInfo
        {
            Length = 80,
            PayloadLength = 80,
            Data = "beef",
        });
        udpDatagramsSentPayload.Ecn.Add(QlogQuicKnownValues.EcnEct0);
        udpDatagramsSentPayload.Ecn.Add(QlogQuicKnownValues.EcnCe);
        udpDatagramsSentPayload.DatagramIds.Add(7);
        udpDatagramsSentPayload.DatagramIds.Add(8);
        udpDatagramsSentPayload.ExtensionData["udp_sent_extension"] = QlogValue.FromString("kept");
        QlogEvent udpDatagramsSent = QlogQuicEvents.CreateUdpDatagramsSent(5, udpDatagramsSentPayload);

        QuicUdpDatagramsReceived udpDatagramsReceivedPayload = new()
        {
            Count = 1,
        };
        udpDatagramsReceivedPayload.Raw.Add(new QuicRawInfo
        {
            Length = 1250,
            PayloadLength = 1200,
        });
        udpDatagramsReceivedPayload.Ecn.Add(QlogQuicKnownValues.EcnNotEct);
        udpDatagramsReceivedPayload.DatagramIds.Add(7);
        udpDatagramsReceivedPayload.ExtensionData["udp_received_extension"] = QlogValue.FromString("kept");
        QlogEvent udpDatagramsReceived = QlogQuicEvents.CreateUdpDatagramsReceived(6, udpDatagramsReceivedPayload);

        QuicUdpDatagramDropped udpDatagramDroppedPayload = new()
        {
            Raw = new QuicRawInfo
            {
                Length = 64,
                PayloadLength = 64,
                Data = "deadbeef",
            },
        };
        udpDatagramDroppedPayload.ExtensionData["udp_dropped_extension"] = QlogValue.FromString("kept");
        QlogEvent udpDatagramDropped = QlogQuicEvents.CreateUdpDatagramDropped(7, udpDatagramDroppedPayload);

        QlogTrace trace = CreateQuicTrace(
            packetSent,
            packetReceived,
            packetDropped,
            packetBuffered,
            packetsAcked,
            udpDatagramsSent,
            udpDatagramsReceived,
            udpDatagramDropped);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Single(trace.EventSchemas);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, trace.EventSchemas[0]);
        Assert.All(trace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));

        Assert.Equal(
            QlogValue.Parse("""
{"packet_type":"initial","packet_number_length":2,"packet_number":17,"token":{"type":"retry","details":{"token_source":"server"},"raw":{"length":16,"data":"00112233445566778899aabbccddeeff"},"token_extension":"kept"},"length":1250,"version":"00000001","scil":0,"dcil":4,"scid":"0a0b","dcid":"0c0d","header_extension":"kept"}
"""),
            packetSent.Data["header"]);
        Assert.Equal(
            QlogValue.Parse("""[{"frame_type":"crypto","offset":0,"raw":{"length":20,"payload_length":4,"data":"00112233"}}]"""),
            packetSent.Data["frames"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":1250,"payload_length":1200,"data":"aa55"}"""),
            packetSent.Data["raw"]);
        Assert.Equal(QlogValue.FromBoolean(true), packetSent.Data["is_mtu_probe_packet"]);
        Assert.Equal(QlogValue.FromString("kept"), packetSent.Data["packet_sent_extension"]);

        Assert.Equal(
            QlogValue.Parse("""{"packet_type":"version_negotiation","header_extension":"kept"}"""),
            packetReceived.Data["header"]);
        Assert.Equal(
            QlogValue.Parse("""["00000001","00000002"]"""),
            packetReceived.Data["supported_versions"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":1250,"payload_length":1200,"data":"bada55"}"""),
            packetReceived.Data["raw"]);
        Assert.Equal(QlogValue.FromNumber(8), packetReceived.Data["datagram_id"]);
        Assert.Equal(QlogValue.FromString("kept"), packetReceived.Data["packet_received_extension"]);

        Assert.Equal(
            QlogValue.Parse("""{"packet_type":"unknown","packet_type_bytes":99,"header_extension":"kept"}"""),
            packetDropped.Data["header"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":64,"payload_length":64}"""),
            packetDropped.Data["raw"]);
        Assert.Equal(
            QlogValue.Parse("""{"reason":"invalid_crc","retry_count":2}"""),
            packetDropped.Data["details"]);
        Assert.Equal(QlogValue.FromNumber(9), packetDropped.Data["datagram_id"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.PacketDroppedTriggerInvalid), packetDropped.Data["trigger"]);

        Assert.Equal(
            QlogValue.Parse("""{"packet_type":"handshake","packet_number_length":2,"packet_number":5,"header_extension":"kept"}"""),
            packetBuffered.Data["header"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":80}"""),
            packetBuffered.Data["raw"]);
        Assert.Equal(QlogValue.FromNumber(10), packetBuffered.Data["datagram_id"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.PacketBufferedTriggerBackpressure), packetBuffered.Data["trigger"]);

        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.PacketNumberSpaceApplicationData), packetsAcked.Data["packet_number_space"]);
        Assert.Equal(QlogValue.Parse("""[17,18,19]"""), packetsAcked.Data["packet_numbers"]);
        Assert.Equal(QlogValue.FromString("kept"), packetsAcked.Data["packets_acked_extension"]);

        Assert.Equal(QlogValue.FromNumber(2), udpDatagramsSent.Data["count"]);
        Assert.Equal(
            QlogValue.Parse("""[{"length":1250,"payload_length":1200},{"length":80,"payload_length":80,"data":"beef"}]"""),
            udpDatagramsSent.Data["raw"]);
        Assert.Equal(QlogValue.Parse("""["ECT(0)","CE"]"""), udpDatagramsSent.Data["ecn"]);
        Assert.Equal(QlogValue.Parse("""[7,8]"""), udpDatagramsSent.Data["datagram_ids"]);
        Assert.Equal(QlogValue.FromString("kept"), udpDatagramsSent.Data["udp_sent_extension"]);

        Assert.Equal(QlogValue.FromNumber(1), udpDatagramsReceived.Data["count"]);
        Assert.Equal(
            QlogValue.Parse("""[{"length":1250,"payload_length":1200}]"""),
            udpDatagramsReceived.Data["raw"]);
        Assert.Equal(QlogValue.Parse("""["Not-ECT"]"""), udpDatagramsReceived.Data["ecn"]);
        Assert.Equal(QlogValue.Parse("""[7]"""), udpDatagramsReceived.Data["datagram_ids"]);
        Assert.Equal(QlogValue.FromString("kept"), udpDatagramsReceived.Data["udp_received_extension"]);

        Assert.Equal(
            QlogValue.Parse("""{"length":64,"payload_length":64,"data":"deadbeef"}"""),
            udpDatagramDropped.Data["raw"]);
        Assert.Equal(QlogValue.FromString("kept"), udpDatagramDropped.Data["udp_dropped_extension"]);

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
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[0].Data["packet_sent_extension"]);
        Assert.Equal(QlogValue.Parse("""["00000001","00000002"]"""), parsedTrace.Events[1].Data["supported_versions"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[7].Data["udp_dropped_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P3-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicStreamAndDatagramMovementEventsAndRoundTripsTheDraftShape()
    {
        QuicStreamStateUpdated streamStateUpdatedPayload = new()
        {
            StreamId = 4,
            StreamType = QlogQuicKnownValues.StreamTypeBidirectional,
            Old = "open",
            New = "half_closed_remote",
            StreamSide = QlogQuicKnownValues.StreamSideReceiving,
            Trigger = QlogQuicKnownValues.StreamStateTriggerRemote,
        };
        streamStateUpdatedPayload.ExtensionData["stream_state_extension"] = QlogValue.FromString("kept");
        QlogEvent streamStateUpdated = QlogQuicEvents.CreateStreamStateUpdated(0, streamStateUpdatedPayload);

        QuicStreamDataMoved streamDataMovedPayload = new()
        {
            StreamId = 4,
            Offset = 128,
            From = QlogQuicKnownValues.DataLocationTransport,
            To = QlogQuicKnownValues.DataLocationApplication,
            AdditionalInfo = QlogQuicKnownValues.DataMovedAdditionalInfoFinSet,
            Raw = new QuicRawInfo
            {
                Length = 64,
                PayloadLength = 64,
            },
        };
        streamDataMovedPayload.ExtensionData["stream_data_extension"] = QlogValue.FromString("kept");
        QlogEvent streamDataMoved = QlogQuicEvents.CreateStreamDataMoved(1, streamDataMovedPayload);

        QuicDatagramDataMoved datagramDataMovedPayload = new()
        {
            From = QlogQuicKnownValues.DataLocationNetwork,
            To = QlogQuicKnownValues.DataLocationApplication,
            Raw = new QuicRawInfo
            {
                Length = 42,
            },
        };
        datagramDataMovedPayload.ExtensionData["datagram_data_extension"] = QlogValue.FromString("kept");
        QlogEvent datagramDataMoved = QlogQuicEvents.CreateDatagramDataMoved(2, datagramDataMovedPayload);

        QlogTrace trace = CreateQuicTrace(streamStateUpdated, streamDataMoved, datagramDataMoved);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Equal(QlogValue.FromNumber(4), streamStateUpdated.Data["stream_id"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.StreamTypeBidirectional), streamStateUpdated.Data["stream_type"]);
        Assert.Equal(QlogValue.FromString("open"), streamStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString("half_closed_remote"), streamStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.StreamSideReceiving), streamStateUpdated.Data["stream_side"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.StreamStateTriggerRemote), streamStateUpdated.Data["trigger"]);
        Assert.Equal(QlogValue.FromString("kept"), streamStateUpdated.Data["stream_state_extension"]);

        Assert.Equal(QlogValue.FromNumber(4), streamDataMoved.Data["stream_id"]);
        Assert.Equal(QlogValue.FromNumber(128), streamDataMoved.Data["offset"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationTransport), streamDataMoved.Data["from"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationApplication), streamDataMoved.Data["to"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataMovedAdditionalInfoFinSet), streamDataMoved.Data["additional_info"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":64,"payload_length":64}"""),
            streamDataMoved.Data["raw"]);
        Assert.Equal(QlogValue.FromString("kept"), streamDataMoved.Data["stream_data_extension"]);

        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationNetwork), datagramDataMoved.Data["from"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.DataLocationApplication), datagramDataMoved.Data["to"]);
        Assert.Equal(
            QlogValue.Parse("""{"length":42}"""),
            datagramDataMoved.Data["raw"]);
        Assert.Equal(QlogValue.FromString("kept"), datagramDataMoved.Data["datagram_data_extension"]);

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
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[0].Data["stream_state_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[1].Data["stream_data_extension"]);
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[2].Data["datagram_data_extension"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P2-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P3-0001")]
    [Trait("CoverageType", "Negative")]
    public void CreateTransportActivityEvents_RejectsMissingRequiredTypedFields()
    {
        Assert.Throws<ArgumentNullException>(() => QlogQuicEvents.CreatePacketSent(0, new QuicPacketSent()));
        Assert.Throws<ArgumentNullException>(() => QlogQuicEvents.CreatePacketReceived(0, new QuicPacketReceived()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreatePacketDropped(0, new QuicPacketDropped { Header = new QuicPacketHeader() }));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateStreamStateUpdated(0, new QuicStreamStateUpdated()));
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
