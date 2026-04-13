using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_QUIC_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S2-0001")]
    [Trait("CoverageType", "Edge")]
    public void RegisterDraftSchema_IsIdempotentAndPreservesTheDraftUri()
    {
        QlogTrace trace = new();

        QlogQuicEvents.RegisterDraftSchema(trace);
        QlogQuicEvents.RegisterDraftSchema(trace);

        Assert.Single(trace.EventSchemas);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, trace.EventSchemas[0]);
        Assert.DoesNotContain(new Uri("urn:ietf:params:qlog:events:quic"), trace.EventSchemas);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S2-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S4-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicLifecycleEventsAndRoundTripsTheDraftShape()
    {
        QuicServerListening serverListeningPayload = new()
        {
            IpV4 = "203.0.113.1",
            PortV4 = 443,
            RetryRequired = true,
        };
        serverListeningPayload.ExtensionData["server_extension"] = QlogValue.FromString("kept");
        QlogEvent serverListening = QlogQuicEvents.CreateServerListening(0, serverListeningPayload);

        QuicTupleEndpointInfo localEndpoint = new()
        {
            IpV4 = "203.0.113.1",
            PortV4 = 443,
        };
        localEndpoint.ConnectionIds.Add("0a0b");
        localEndpoint.ExtensionData["endpoint_extension"] = QlogValue.FromString("kept");

        QuicTupleEndpointInfo remoteEndpoint = new()
        {
            IpV4 = "198.51.100.2",
            PortV4 = 443,
        };
        remoteEndpoint.ConnectionIds.Add("0c0d");

        QuicConnectionStarted connectionStartedPayload = new()
        {
            Local = localEndpoint,
            Remote = remoteEndpoint,
        };
        connectionStartedPayload.ExtensionData["event_extension"] = QlogValue.FromString("kept");
        QlogEvent connectionStarted = QlogQuicEvents.CreateConnectionStarted(1, connectionStartedPayload);

        QuicConnectionStateUpdated connectionStateUpdatedPayload = new()
        {
            Old = QlogQuicKnownValues.ConnectionStateHandshakeStarted,
            New = QlogQuicKnownValues.ConnectionStateHandshakeComplete,
        };
        connectionStateUpdatedPayload.ExtensionData["state_extension"] = QlogValue.FromString("kept");
        QlogEvent connectionStateUpdated = QlogQuicEvents.CreateConnectionStateUpdated(2, connectionStateUpdatedPayload);

        QuicConnectionClosed connectionClosedPayload = new()
        {
            Initiator = QlogQuicKnownValues.RemoteInitiator,
            ConnectionError = "unknown",
            ErrorCode = 42,
            Reason = "application requested shutdown",
            Trigger = QlogQuicKnownValues.CloseTriggerApplication,
        };
        connectionClosedPayload.ExtensionData["closed_extension"] = QlogValue.FromString("kept");
        QlogEvent connectionClosed = QlogQuicEvents.CreateConnectionClosed(3, connectionClosedPayload);

        QuicTupleAssigned tupleAssignedPayload = new()
        {
            TupleId = string.Empty,
            TupleRemote = new QuicTupleEndpointInfo
            {
                IpV4 = "198.51.100.2",
                PortV4 = 443,
            },
            TupleLocal = new QuicTupleEndpointInfo
            {
                IpV4 = "203.0.113.1",
                PortV4 = 443,
            },
        };
        tupleAssignedPayload.TupleLocal!.ExtensionData["tuple_extension"] = QlogValue.FromString("kept");
        tupleAssignedPayload.ExtensionData["tuple_assigned_extension"] = QlogValue.FromString("kept");
        QlogEvent tupleAssigned = QlogQuicEvents.CreateTupleAssigned(4, tupleAssignedPayload);

        QlogTrace trace = CreateQuicTrace(serverListening, connectionStarted, connectionStateUpdated, connectionClosed, tupleAssigned);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Single(trace.EventSchemas);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, trace.EventSchemas[0]);
        Assert.All(trace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));

        Assert.Equal(QlogValue.FromString("kept"), serverListening.Data["server_extension"]);
        Assert.Equal(QlogValue.Parse("""{"ip_v4":"203.0.113.1","port_v4":443,"connection_ids":["0a0b"],"endpoint_extension":"kept"}"""), connectionStarted.Data["local"]);
        Assert.Equal(QlogValue.Parse("""{"ip_v4":"198.51.100.2","port_v4":443,"connection_ids":["0c0d"]}"""), connectionStarted.Data["remote"]);
        Assert.Equal(QlogValue.FromString("kept"), connectionStarted.Data["event_extension"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.ConnectionStateHandshakeStarted), connectionStateUpdated.Data["old"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.ConnectionStateHandshakeComplete), connectionStateUpdated.Data["new"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.RemoteInitiator), connectionClosed.Data["initiator"]);
        Assert.Equal(QlogValue.FromString("unknown"), connectionClosed.Data["connection_error"]);
        Assert.Equal(QlogValue.FromNumber(42), connectionClosed.Data["error_code"]);
        Assert.Equal(QlogValue.FromString("application requested shutdown"), connectionClosed.Data["reason"]);
        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.CloseTriggerApplication), connectionClosed.Data["trigger"]);
        Assert.Equal(QlogValue.FromString("kept"), tupleAssigned.Data["tuple_assigned_extension"]);
        Assert.Equal(QlogValue.FromString(""), tupleAssigned.Data["tuple_id"]);
        Assert.Equal(QlogValue.Parse("""{"ip_v4":"203.0.113.1","port_v4":443,"tuple_extension":"kept"}"""), tupleAssigned.Data["tuple_local"]);
        Assert.Equal(QlogValue.Parse("""{"ip_v4":"198.51.100.2","port_v4":443}"""), tupleAssigned.Data["tuple_remote"]);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);

        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces.Single());
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(parsedTrace.EventSchemas));
        Assert.All(parsedTrace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[0].Data["server_extension"]);
        Assert.Equal(QlogValue.FromString(""), parsedTrace.Events[4].Data["tuple_id"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S4-0001")]
    [Trait("CoverageType", "Negative")]
    public void CreateLifecycleEvents_RejectsMissingRequiredFields()
    {
        Assert.Throws<ArgumentNullException>(() => QlogQuicEvents.CreateConnectionStarted(1, new QuicConnectionStarted()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateConnectionStateUpdated(1, new QuicConnectionStateUpdated()));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateTupleAssigned(1, new QuicTupleAssigned { TupleId = " " }));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S2-0001")]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P1-0001")]
    [Trait("CoverageType", "Positive")]
    public void Serialize_WritesQuicNegotiationEventsAndRoundTripsTheDraftShape()
    {
        QuicVersionInformation versionInformationPayload = new()
        {
            ChosenVersion = "00000002",
        };
        versionInformationPayload.ServerVersions.Add("00000001");
        versionInformationPayload.ServerVersions.Add("00000002");
        versionInformationPayload.ClientVersions.Add("00000001");
        versionInformationPayload.ClientVersions.Add("00000002");
        versionInformationPayload.ExtensionData["version_extension"] = QlogValue.FromString("kept");
        QlogEvent versionInformation = QlogQuicEvents.CreateVersionInformation(0, versionInformationPayload);

        QuicAlpnInformation alpnInformationPayload = new()
        {
            ChosenAlpn = new QuicAlpnIdentifier
            {
                StringValue = "h3",
            },
        };
        alpnInformationPayload.ServerAlpns.Add(new QuicAlpnIdentifier
        {
            StringValue = "h3",
        });
        alpnInformationPayload.ClientAlpns.Add(new QuicAlpnIdentifier
        {
            StringValue = "h3",
        });
        alpnInformationPayload.ClientAlpns.Add(new QuicAlpnIdentifier
        {
            ByteValue = "6833",
        });
        alpnInformationPayload.ExtensionData["alpn_extension"] = QlogValue.FromString("kept");
        QlogEvent alpnInformation = QlogQuicEvents.CreateAlpnInformation(1, alpnInformationPayload);

        QuicParametersSet parametersSetPayload = new()
        {
            Initiator = QlogQuicKnownValues.LocalInitiator,
            ResumptionAllowed = true,
            EarlyDataEnabled = true,
            TlsCipher = "AES_128_GCM_SHA256",
            OriginalDestinationConnectionId = "0a0b",
            InitialSourceConnectionId = "0c0d",
            RetrySourceConnectionId = "0e0f",
            StatelessResetToken = "00112233445566778899aabbccddeeff",
            DisableActiveMigration = true,
            MaxIdleTimeout = 30,
            MaxUdpPayloadSize = 1350,
            AckDelayExponent = 3,
            MaxAckDelay = 25,
            ActiveConnectionIdLimit = 4,
            InitialMaxData = 65536,
            InitialMaxStreamDataBidiLocal = 16384,
            InitialMaxStreamDataBidiRemote = 16384,
            InitialMaxStreamDataUni = 8192,
            InitialMaxStreamsBidi = 16,
            InitialMaxStreamsUni = 8,
            PreferredAddress = new QuicPreferredAddress
            {
                ConnectionId = "deadbeef",
                StatelessResetToken = "00112233445566778899aabbccddeeff",
                IpV4 = "203.0.113.10",
                PortV4 = 443,
                IpV6 = "2001:db8::10",
                PortV6 = 443,
            },
            MaxDatagramFrameSize = 1200,
            GreaseQuicBit = true,
        };
        parametersSetPayload.UnknownParameters.Add(new QuicUnknownParameter
        {
            Id = 42,
            Value = "c0de",
        });
        parametersSetPayload.ExtensionData["parameters_extension"] = QlogValue.FromString("kept");
        QlogEvent parametersSet = QlogQuicEvents.CreateParametersSet(2, parametersSetPayload);

        QuicParametersRestored parametersRestoredPayload = new()
        {
            DisableActiveMigration = true,
            MaxIdleTimeout = 30,
            MaxUdpPayloadSize = 1350,
            ActiveConnectionIdLimit = 4,
            InitialMaxData = 65536,
            InitialMaxStreamDataBidiLocal = 16384,
            InitialMaxStreamDataBidiRemote = 16384,
            InitialMaxStreamDataUni = 8192,
            InitialMaxStreamsBidi = 16,
            InitialMaxStreamsUni = 8,
            MaxDatagramFrameSize = 1200,
            GreaseQuicBit = true,
        };
        parametersRestoredPayload.ExtensionData["restored_extension"] = QlogValue.FromString("kept");
        QlogEvent parametersRestored = QlogQuicEvents.CreateParametersRestored(3, parametersRestoredPayload);

        QlogTrace trace = CreateQuicTrace(versionInformation, alpnInformation, parametersSet, parametersRestored);
        QlogFile file = new();
        file.Traces.Add(trace);

        Assert.Single(trace.EventSchemas);
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, trace.EventSchemas[0]);
        Assert.All(trace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));

        Assert.Equal(QlogValue.FromString("kept"), versionInformation.Data["version_extension"]);
        Assert.Equal(QlogValue.Parse("""["00000001","00000002"]"""), versionInformation.Data["server_versions"]);
        Assert.Equal(QlogValue.Parse("""["00000001","00000002"]"""), versionInformation.Data["client_versions"]);
        Assert.Equal(QlogValue.FromString("00000002"), versionInformation.Data["chosen_version"]);

        Assert.Equal(QlogValue.FromString("kept"), alpnInformation.Data["alpn_extension"]);
        Assert.Equal(QlogValue.Parse("""[{"string_value":"h3"},{"byte_value":"6833"}]"""), alpnInformation.Data["client_alpns"]);
        Assert.Equal(QlogValue.Parse("""[{"string_value":"h3"}]"""), alpnInformation.Data["server_alpns"]);
        Assert.Equal(QlogValue.Parse("""{"string_value":"h3"}"""), alpnInformation.Data["chosen_alpn"]);

        Assert.Equal(QlogValue.FromString(QlogQuicKnownValues.LocalInitiator), parametersSet.Data["initiator"]);
        Assert.Equal(QlogValue.FromBoolean(true), parametersSet.Data["resumption_allowed"]);
        Assert.Equal(QlogValue.FromBoolean(true), parametersSet.Data["early_data_enabled"]);
        Assert.Equal(QlogValue.FromString("AES_128_GCM_SHA256"), parametersSet.Data["tls_cipher"]);
        Assert.Equal(QlogValue.FromString("0a0b"), parametersSet.Data["original_destination_connection_id"]);
        Assert.Equal(QlogValue.FromString("0c0d"), parametersSet.Data["initial_source_connection_id"]);
        Assert.Equal(QlogValue.FromString("0e0f"), parametersSet.Data["retry_source_connection_id"]);
        Assert.Equal(QlogValue.FromString("00112233445566778899aabbccddeeff"), parametersSet.Data["stateless_reset_token"]);
        Assert.Equal(QlogValue.FromBoolean(true), parametersSet.Data["disable_active_migration"]);
        Assert.Equal(QlogValue.FromNumber(30), parametersSet.Data["max_idle_timeout"]);
        Assert.Equal(QlogValue.FromNumber(1350), parametersSet.Data["max_udp_payload_size"]);
        Assert.Equal(QlogValue.FromNumber(3), parametersSet.Data["ack_delay_exponent"]);
        Assert.Equal(QlogValue.FromNumber(25), parametersSet.Data["max_ack_delay"]);
        Assert.Equal(QlogValue.FromNumber(4), parametersSet.Data["active_connection_id_limit"]);
        Assert.Equal(QlogValue.FromNumber(65536), parametersSet.Data["initial_max_data"]);
        Assert.Equal(QlogValue.FromNumber(16384), parametersSet.Data["initial_max_stream_data_bidi_local"]);
        Assert.Equal(QlogValue.FromNumber(16384), parametersSet.Data["initial_max_stream_data_bidi_remote"]);
        Assert.Equal(QlogValue.FromNumber(8192), parametersSet.Data["initial_max_stream_data_uni"]);
        Assert.Equal(QlogValue.FromNumber(16), parametersSet.Data["initial_max_streams_bidi"]);
        Assert.Equal(QlogValue.FromNumber(8), parametersSet.Data["initial_max_streams_uni"]);
        Assert.Equal(
            QlogValue.Parse("""{"ip_v4":"203.0.113.10","port_v4":443,"ip_v6":"2001:db8::10","port_v6":443,"connection_id":"deadbeef","stateless_reset_token":"00112233445566778899aabbccddeeff"}"""),
            parametersSet.Data["preferred_address"]);
        Assert.Equal(QlogValue.Parse("""[{"id":42,"value":"c0de"}]"""), parametersSet.Data["unknown_parameters"]);
        Assert.Equal(QlogValue.FromNumber(1200), parametersSet.Data["max_datagram_frame_size"]);
        Assert.Equal(QlogValue.FromBoolean(true), parametersSet.Data["grease_quic_bit"]);
        Assert.Equal(QlogValue.FromString("kept"), parametersSet.Data["parameters_extension"]);

        Assert.Equal(QlogValue.FromBoolean(true), parametersRestored.Data["disable_active_migration"]);
        Assert.Equal(QlogValue.FromNumber(30), parametersRestored.Data["max_idle_timeout"]);
        Assert.Equal(QlogValue.FromNumber(1350), parametersRestored.Data["max_udp_payload_size"]);
        Assert.Equal(QlogValue.FromNumber(4), parametersRestored.Data["active_connection_id_limit"]);
        Assert.Equal(QlogValue.FromNumber(65536), parametersRestored.Data["initial_max_data"]);
        Assert.Equal(QlogValue.FromNumber(16384), parametersRestored.Data["initial_max_stream_data_bidi_local"]);
        Assert.Equal(QlogValue.FromNumber(16384), parametersRestored.Data["initial_max_stream_data_bidi_remote"]);
        Assert.Equal(QlogValue.FromNumber(8192), parametersRestored.Data["initial_max_stream_data_uni"]);
        Assert.Equal(QlogValue.FromNumber(16), parametersRestored.Data["initial_max_streams_bidi"]);
        Assert.Equal(QlogValue.FromNumber(8), parametersRestored.Data["initial_max_streams_uni"]);
        Assert.Equal(QlogValue.FromNumber(1200), parametersRestored.Data["max_datagram_frame_size"]);
        Assert.Equal(QlogValue.FromBoolean(true), parametersRestored.Data["grease_quic_bit"]);
        Assert.Equal(QlogValue.FromString("kept"), parametersRestored.Data["restored_extension"]);

        string json = QlogJsonSerializer.Serialize(file);
        QlogFile parsed = QlogJsonSerializer.Deserialize(json);
        string roundTrippedJson = QlogJsonSerializer.Serialize(parsed);

        Assert.Equal(json, roundTrippedJson);

        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces.Single());
        Assert.Equal(QlogQuicKnownValues.DraftEventSchemaUri, Assert.Single(parsedTrace.EventSchemas));
        Assert.All(parsedTrace.Events, qlogEvent => Assert.StartsWith(QlogQuicKnownValues.EventNamespace + ":", qlogEvent.Name, StringComparison.Ordinal));
        Assert.Equal(QlogValue.FromString("kept"), parsedTrace.Events[2].Data["parameters_extension"]);
        Assert.Equal(QlogValue.Parse("""{"string_value":"h3"}"""), parsedTrace.Events[1].Data["chosen_alpn"]);
        Assert.Equal(QlogValue.Parse("""[{"id":42,"value":"c0de"}]"""), parsedTrace.Events[2].Data["unknown_parameters"]);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-QUIC-S5P1-0001")]
    [Trait("CoverageType", "Negative")]
    public void CreateNegotiationEvents_RejectsInvalidTypedPayloadShapes()
    {
        QuicVersionInformation invalidVersions = new();
        invalidVersions.ServerVersions.Add(string.Empty);

        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateVersionInformation(1, invalidVersions));
        Assert.Throws<InvalidOperationException>(() => QlogQuicEvents.CreateAlpnInformation(1, new QuicAlpnInformation
        {
            ChosenAlpn = new QuicAlpnIdentifier(),
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
