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
    /// Gets the local initiator identifier.
    /// </summary>
    public const string LocalInitiator = "local";

    /// <summary>
    /// Gets the remote initiator identifier.
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
}
