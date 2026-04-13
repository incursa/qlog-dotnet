namespace Incursa.Qlog.Quic;

/// <summary>
/// Provides draft-known QUIC qlog values for the bounded QUIC vocabulary slices.
/// </summary>
public static class QlogQuicKnownValues
{
    /// <summary>
    /// Gets the qlog QUIC namespace identifier.
    /// </summary>
    public const string EventNamespace = "quic";

    /// <summary>
    /// Gets the draft QUIC event schema URI required by the current baseline.
    /// </summary>
    public static Uri DraftEventSchemaUri { get; } = new("urn:ietf:params:qlog:events:quic-12", UriKind.Absolute);

    /// <summary>
    /// Gets the fully qualified event name for <c>server_listening</c>.
    /// </summary>
    public const string ServerListeningEventName = "quic:server_listening";

    /// <summary>
    /// Gets the fully qualified event name for <c>connection_started</c>.
    /// </summary>
    public const string ConnectionStartedEventName = "quic:connection_started";

    /// <summary>
    /// Gets the fully qualified event name for <c>connection_closed</c>.
    /// </summary>
    public const string ConnectionClosedEventName = "quic:connection_closed";

    /// <summary>
    /// Gets the fully qualified event name for <c>connection_id_updated</c>.
    /// </summary>
    public const string ConnectionIdUpdatedEventName = "quic:connection_id_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>connection_state_updated</c>.
    /// </summary>
    public const string ConnectionStateUpdatedEventName = "quic:connection_state_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>tuple_assigned</c>.
    /// </summary>
    public const string TupleAssignedEventName = "quic:tuple_assigned";

    /// <summary>
    /// Gets the fully qualified event name for <c>version_information</c>.
    /// </summary>
    public const string VersionInformationEventName = "quic:version_information";

    /// <summary>
    /// Gets the fully qualified event name for <c>alpn_information</c>.
    /// </summary>
    public const string AlpnInformationEventName = "quic:alpn_information";

    /// <summary>
    /// Gets the fully qualified event name for <c>parameters_set</c>.
    /// </summary>
    public const string ParametersSetEventName = "quic:parameters_set";

    /// <summary>
    /// Gets the fully qualified event name for <c>parameters_restored</c>.
    /// </summary>
    public const string ParametersRestoredEventName = "quic:parameters_restored";

    /// <summary>
    /// Gets the fully qualified event name for <c>packet_sent</c>.
    /// </summary>
    public const string PacketSentEventName = "quic:packet_sent";

    /// <summary>
    /// Gets the fully qualified event name for <c>packet_received</c>.
    /// </summary>
    public const string PacketReceivedEventName = "quic:packet_received";

    /// <summary>
    /// Gets the fully qualified event name for <c>packet_dropped</c>.
    /// </summary>
    public const string PacketDroppedEventName = "quic:packet_dropped";

    /// <summary>
    /// Gets the fully qualified event name for <c>packet_buffered</c>.
    /// </summary>
    public const string PacketBufferedEventName = "quic:packet_buffered";

    /// <summary>
    /// Gets the fully qualified event name for <c>packets_acked</c>.
    /// </summary>
    public const string PacketsAckedEventName = "quic:packets_acked";

    /// <summary>
    /// Gets the fully qualified event name for <c>udp_datagrams_sent</c>.
    /// </summary>
    public const string UdpDatagramsSentEventName = "quic:udp_datagrams_sent";

    /// <summary>
    /// Gets the fully qualified event name for <c>udp_datagrams_received</c>.
    /// </summary>
    public const string UdpDatagramsReceivedEventName = "quic:udp_datagrams_received";

    /// <summary>
    /// Gets the fully qualified event name for <c>udp_datagram_dropped</c>.
    /// </summary>
    public const string UdpDatagramDroppedEventName = "quic:udp_datagram_dropped";

    /// <summary>
    /// Gets the fully qualified event name for <c>stream_state_updated</c>.
    /// </summary>
    public const string StreamStateUpdatedEventName = "quic:stream_state_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>stream_data_moved</c>.
    /// </summary>
    public const string StreamDataMovedEventName = "quic:stream_data_moved";

    /// <summary>
    /// Gets the fully qualified event name for <c>datagram_data_moved</c>.
    /// </summary>
    public const string DatagramDataMovedEventName = "quic:datagram_data_moved";

    /// <summary>
    /// Gets the fully qualified event name for <c>migration_state_updated</c>.
    /// </summary>
    public const string MigrationStateUpdatedEventName = "quic:migration_state_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>key_updated</c>.
    /// </summary>
    public const string KeyUpdatedEventName = "quic:key_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>key_discarded</c>.
    /// </summary>
    public const string KeyDiscardedEventName = "quic:key_discarded";

    /// <summary>
    /// Gets the fully qualified event name for <c>recovery_parameters_set</c>.
    /// </summary>
    public const string RecoveryParametersSetEventName = "quic:recovery_parameters_set";

    /// <summary>
    /// Gets the fully qualified event name for <c>recovery_metrics_updated</c>.
    /// </summary>
    public const string RecoveryMetricsUpdatedEventName = "quic:recovery_metrics_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>congestion_state_updated</c>.
    /// </summary>
    public const string CongestionStateUpdatedEventName = "quic:congestion_state_updated";

    /// <summary>
    /// Gets the fully qualified event name for <c>packet_lost</c>.
    /// </summary>
    public const string PacketLostEventName = "quic:packet_lost";

    /// <summary>
    /// Gets the qlog packet type identifier for an initial packet.
    /// </summary>
    public const string PacketTypeInitial = "initial";

    /// <summary>
    /// Gets the qlog packet type identifier for a handshake packet.
    /// </summary>
    public const string PacketTypeHandshake = "handshake";

    /// <summary>
    /// Gets the qlog packet type identifier for a 0-RTT packet.
    /// </summary>
    public const string PacketTypeZeroRtt = "0RTT";

    /// <summary>
    /// Gets the qlog packet type identifier for a 1-RTT packet.
    /// </summary>
    public const string PacketTypeOneRtt = "1RTT";

    /// <summary>
    /// Gets the qlog packet type identifier for a retry packet.
    /// </summary>
    public const string PacketTypeRetry = "retry";

    /// <summary>
    /// Gets the qlog packet type identifier for a version-negotiation packet.
    /// </summary>
    public const string PacketTypeVersionNegotiation = "version_negotiation";

    /// <summary>
    /// Gets the qlog packet type identifier for a stateless-reset packet.
    /// </summary>
    public const string PacketTypeStatelessReset = "stateless_reset";

    /// <summary>
    /// Gets the qlog packet type identifier for an unknown packet type.
    /// </summary>
    public const string PacketTypeUnknown = "unknown";

    /// <summary>
    /// Gets the packet-sent trigger for a reordered retransmission.
    /// </summary>
    public const string PacketSentTriggerRetransmitReordered = "retransmit_reordered";

    /// <summary>
    /// Gets the packet-sent trigger for a timeout retransmission.
    /// </summary>
    public const string PacketSentTriggerRetransmitTimeout = "retransmit_timeout";

    /// <summary>
    /// Gets the packet-sent trigger for a probe timeout.
    /// </summary>
    public const string PacketSentTriggerPtoProbe = "pto_probe";

    /// <summary>
    /// Gets the packet-sent trigger for a crypto retransmission.
    /// </summary>
    public const string PacketSentTriggerRetransmitCrypto = "retransmit_crypto";

    /// <summary>
    /// Gets the packet-sent trigger for a congestion-control bandwidth probe.
    /// </summary>
    public const string PacketSentTriggerCcBandwidthProbe = "cc_bandwidth_probe";

    /// <summary>
    /// Gets the packet-received trigger for packets that became processable after keys arrived.
    /// </summary>
    public const string PacketReceivedTriggerKeysAvailable = "keys_available";

    /// <summary>
    /// Gets the packet-dropped trigger for internal errors.
    /// </summary>
    public const string PacketDroppedTriggerInternalError = "internal_error";

    /// <summary>
    /// Gets the packet-dropped trigger for rejected packets.
    /// </summary>
    public const string PacketDroppedTriggerRejected = "rejected";

    /// <summary>
    /// Gets the packet-dropped trigger for unsupported packets.
    /// </summary>
    public const string PacketDroppedTriggerUnsupported = "unsupported";

    /// <summary>
    /// Gets the packet-dropped trigger for invalid packets.
    /// </summary>
    public const string PacketDroppedTriggerInvalid = "invalid";

    /// <summary>
    /// Gets the packet-dropped trigger for duplicate packets.
    /// </summary>
    public const string PacketDroppedTriggerDuplicate = "duplicate";

    /// <summary>
    /// Gets the packet-dropped trigger for packets that could not be matched to a connection.
    /// </summary>
    public const string PacketDroppedTriggerConnectionUnknown = "connection_unknown";

    /// <summary>
    /// Gets the packet-dropped trigger for decryption failures.
    /// </summary>
    public const string PacketDroppedTriggerDecryptionFailure = "decryption_failure";

    /// <summary>
    /// Gets the packet-dropped trigger for unavailable keys.
    /// </summary>
    public const string PacketDroppedTriggerKeyUnavailable = "key_unavailable";

    /// <summary>
    /// Gets the packet-dropped trigger for general packet drops.
    /// </summary>
    public const string PacketDroppedTriggerGeneral = "general";

    /// <summary>
    /// Gets the packet-buffered trigger for backpressure.
    /// </summary>
    public const string PacketBufferedTriggerBackpressure = "backpressure";

    /// <summary>
    /// Gets the packet-buffered trigger for unavailable keys.
    /// </summary>
    public const string PacketBufferedTriggerKeysUnavailable = "keys_unavailable";

    /// <summary>
    /// Gets the packet-lost trigger for reordering-threshold loss detection.
    /// </summary>
    public const string PacketLostTriggerReorderingThreshold = "reordering_threshold";

    /// <summary>
    /// Gets the packet-lost trigger for time-threshold loss detection.
    /// </summary>
    public const string PacketLostTriggerTimeThreshold = "time_threshold";

    /// <summary>
    /// Gets the packet-lost trigger for PTO-expired loss detection.
    /// </summary>
    public const string PacketLostTriggerPtoExpired = "pto_expired";

    /// <summary>
    /// Gets the packet-number space for initial packets.
    /// </summary>
    public const string PacketNumberSpaceInitial = "initial";

    /// <summary>
    /// Gets the packet-number space for handshake packets.
    /// </summary>
    public const string PacketNumberSpaceHandshake = "handshake";

    /// <summary>
    /// Gets the packet-number space for application-data packets.
    /// </summary>
    public const string PacketNumberSpaceApplicationData = "application_data";

    /// <summary>
    /// Gets the token type for retry tokens.
    /// </summary>
    public const string TokenTypeRetry = "retry";

    /// <summary>
    /// Gets the token type for resumption tokens.
    /// </summary>
    public const string TokenTypeResumption = "resumption";

    /// <summary>
    /// Gets the stream type for unidirectional streams.
    /// </summary>
    public const string StreamTypeUnidirectional = "unidirectional";

    /// <summary>
    /// Gets the stream type for bidirectional streams.
    /// </summary>
    public const string StreamTypeBidirectional = "bidirectional";

    /// <summary>
    /// Gets the stream side for send-state updates.
    /// </summary>
    public const string StreamSideSending = "sending";

    /// <summary>
    /// Gets the stream side for receive-state updates.
    /// </summary>
    public const string StreamSideReceiving = "receiving";

    /// <summary>
    /// Gets the stream-state trigger for local actions.
    /// </summary>
    public const string StreamStateTriggerLocal = "local";

    /// <summary>
    /// Gets the stream-state trigger for remote actions.
    /// </summary>
    public const string StreamStateTriggerRemote = "remote";

    /// <summary>
    /// Gets the data-location identifier for application-layer buffers.
    /// </summary>
    public const string DataLocationApplication = "application";

    /// <summary>
    /// Gets the data-location identifier for transport-layer buffers.
    /// </summary>
    public const string DataLocationTransport = "transport";

    /// <summary>
    /// Gets the data-location identifier for network-layer buffers.
    /// </summary>
    public const string DataLocationNetwork = "network";

    /// <summary>
    /// Gets the additional-info marker for a finished stream.
    /// </summary>
    public const string DataMovedAdditionalInfoFinSet = "fin_set";

    /// <summary>
    /// Gets the additional-info marker for a stream reset.
    /// </summary>
    public const string DataMovedAdditionalInfoStreamReset = "stream_reset";

    /// <summary>
    /// Gets the ECN value for non-ECN-marked packets.
    /// </summary>
    public const string EcnNotEct = "Not-ECT";

    /// <summary>
    /// Gets the ECN value for ECT(1)-marked packets.
    /// </summary>
    public const string EcnEct1 = "ECT(1)";

    /// <summary>
    /// Gets the ECN value for ECT(0)-marked packets.
    /// </summary>
    public const string EcnEct0 = "ECT(0)";

    /// <summary>
    /// Gets the ECN value for congestion-experienced packets.
    /// </summary>
    public const string EcnCe = "CE";

    /// <summary>
    /// Gets the client initiator identifier.
    /// </summary>
    public const string LocalInitiator = "local";

    /// <summary>
    /// Gets the server initiator identifier.
    /// </summary>
    public const string RemoteInitiator = "remote";

    /// <summary>
    /// Gets the base connection state for an attempted connection.
    /// </summary>
    public const string ConnectionStateAttempted = "attempted";

    /// <summary>
    /// Gets the base connection state for a started handshake.
    /// </summary>
    public const string ConnectionStateHandshakeStarted = "handshake_started";

    /// <summary>
    /// Gets the base connection state for a completed handshake.
    /// </summary>
    public const string ConnectionStateHandshakeComplete = "handshake_complete";

    /// <summary>
    /// Gets the granular connection state for peer validation.
    /// </summary>
    public const string ConnectionStatePeerValidated = "peer_validated";

    /// <summary>
    /// Gets the granular connection state for early server writes.
    /// </summary>
    public const string ConnectionStateEarlyWrite = "early_write";

    /// <summary>
    /// Gets the granular connection state for a confirmed handshake.
    /// </summary>
    public const string ConnectionStateHandshakeConfirmed = "handshake_confirmed";

    /// <summary>
    /// Gets the granular connection state for a locally closing connection.
    /// </summary>
    public const string ConnectionStateClosing = "closing";

    /// <summary>
    /// Gets the granular connection state for a draining connection.
    /// </summary>
    public const string ConnectionStateDraining = "draining";

    /// <summary>
    /// Gets the closed connection state.
    /// </summary>
    public const string ConnectionStateClosed = "closed";

    /// <summary>
    /// Gets the connection-close trigger for idle timeouts.
    /// </summary>
    public const string CloseTriggerIdleTimeout = "idle_timeout";

    /// <summary>
    /// Gets the connection-close trigger for application-initiated closes.
    /// </summary>
    public const string CloseTriggerApplication = "application";

    /// <summary>
    /// Gets the connection-close trigger for transport or crypto errors.
    /// </summary>
    public const string CloseTriggerError = "error";

    /// <summary>
    /// Gets the connection-close trigger for version mismatches.
    /// </summary>
    public const string CloseTriggerVersionMismatch = "version_mismatch";

    /// <summary>
    /// Gets the connection-close trigger for stateless resets.
    /// </summary>
    public const string CloseTriggerStatelessReset = "stateless_reset";

    /// <summary>
    /// Gets the connection-close trigger for aborted connections.
    /// </summary>
    public const string CloseTriggerAborted = "aborted";

    /// <summary>
    /// Gets the connection-close trigger when the implementation cannot determine a more precise reason.
    /// </summary>
    public const string CloseTriggerUnspecified = "unspecified";

    /// <summary>
    /// Gets the migration state for the start of probing.
    /// </summary>
    public const string MigrationStateProbingStarted = "probing_started";

    /// <summary>
    /// Gets the migration state for an abandoned probe attempt.
    /// </summary>
    public const string MigrationStateProbingAbandoned = "probing_abandoned";

    /// <summary>
    /// Gets the migration state for a successful probe.
    /// </summary>
    public const string MigrationStateProbingSuccessful = "probing_successful";

    /// <summary>
    /// Gets the migration state for an active migration attempt.
    /// </summary>
    public const string MigrationStateStarted = "migration_started";

    /// <summary>
    /// Gets the migration state for an abandoned migration attempt.
    /// </summary>
    public const string MigrationStateAbandoned = "migration_abandoned";

    /// <summary>
    /// Gets the migration state for a completed migration.
    /// </summary>
    public const string MigrationStateComplete = "migration_complete";

    /// <summary>
    /// Gets the key type for the server initial secret.
    /// </summary>
    public const string KeyTypeServerInitialSecret = "server_initial_secret";

    /// <summary>
    /// Gets the key type for the client initial secret.
    /// </summary>
    public const string KeyTypeClientInitialSecret = "client_initial_secret";

    /// <summary>
    /// Gets the key type for the server handshake secret.
    /// </summary>
    public const string KeyTypeServerHandshakeSecret = "server_handshake_secret";

    /// <summary>
    /// Gets the key type for the client handshake secret.
    /// </summary>
    public const string KeyTypeClientHandshakeSecret = "client_handshake_secret";

    /// <summary>
    /// Gets the key type for the server 0-RTT secret.
    /// </summary>
    public const string KeyTypeServerZeroRttSecret = "server_0rtt_secret";

    /// <summary>
    /// Gets the key type for the client 0-RTT secret.
    /// </summary>
    public const string KeyTypeClientZeroRttSecret = "client_0rtt_secret";

    /// <summary>
    /// Gets the key type for the server 1-RTT secret.
    /// </summary>
    public const string KeyTypeServerOneRttSecret = "server_1rtt_secret";

    /// <summary>
    /// Gets the key type for the client 1-RTT secret.
    /// </summary>
    public const string KeyTypeClientOneRttSecret = "client_1rtt_secret";

    /// <summary>
    /// Gets the key lifecycle trigger for TLS-derived keys.
    /// </summary>
    public const string KeyLifecycleTriggerTls = "tls";

    /// <summary>
    /// Gets the key lifecycle trigger for a remote update.
    /// </summary>
    public const string KeyLifecycleTriggerRemoteUpdate = "remote_update";

    /// <summary>
    /// Gets the key lifecycle trigger for a local update.
    /// </summary>
    public const string KeyLifecycleTriggerLocalUpdate = "local_update";
}
