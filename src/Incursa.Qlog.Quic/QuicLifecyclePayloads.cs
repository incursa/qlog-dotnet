namespace Incursa.Qlog.Quic;

/// <summary>
/// Represents the payload for <c>quic:server_listening</c>.
/// </summary>
public sealed class QuicServerListening
{
    /// <summary>
    /// Gets or sets the IPv4 address, if known.
    /// </summary>
    public string? IpV4 { get; set; }

    /// <summary>
    /// Gets or sets the IPv4 port, if known.
    /// </summary>
    public ushort? PortV4 { get; set; }

    /// <summary>
    /// Gets or sets the IPv6 address, if known.
    /// </summary>
    public string? IpV6 { get; set; }

    /// <summary>
    /// Gets or sets the IPv6 port, if known.
    /// </summary>
    public ushort? PortV6 { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server always requires retry.
    /// </summary>
    public bool? RetryRequired { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:connection_started</c>.
/// </summary>
public sealed class QuicConnectionStarted
{
    /// <summary>
    /// Gets or sets the local endpoint tuple metadata.
    /// </summary>
    public QuicTupleEndpointInfo? Local { get; set; }

    /// <summary>
    /// Gets or sets the remote endpoint tuple metadata.
    /// </summary>
    public QuicTupleEndpointInfo? Remote { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:connection_closed</c>.
/// </summary>
public sealed class QuicConnectionClosed
{
    /// <summary>
    /// Gets or sets the side that initiated the close, if known.
    /// </summary>
    public string? Initiator { get; set; }

    /// <summary>
    /// Gets or sets the transport or crypto error identifier, if known.
    /// </summary>
    public string? ConnectionError { get; set; }

    /// <summary>
    /// Gets or sets the application error identifier, if known.
    /// </summary>
    public string? ApplicationError { get; set; }

    /// <summary>
    /// Gets or sets the raw error code when the mapped error identifier is unknown.
    /// </summary>
    public ulong? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the implementation-specific internal code, if known.
    /// </summary>
    public ulong? InternalCode { get; set; }

    /// <summary>
    /// Gets or sets the close reason, if known.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the close trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:connection_state_updated</c>.
/// </summary>
public sealed class QuicConnectionStateUpdated
{
    /// <summary>
    /// Gets or sets the previous connection state, if known.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new connection state.
    /// </summary>
    public string New { get; set; } = string.Empty;

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:tuple_assigned</c>.
/// </summary>
public sealed class QuicTupleAssigned
{
    /// <summary>
    /// Gets or sets the tuple identifier.
    /// </summary>
    public string TupleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote tuple-half metadata, if known.
    /// </summary>
    public QuicTupleEndpointInfo? TupleRemote { get; set; }

    /// <summary>
    /// Gets or sets the local tuple-half metadata, if known.
    /// </summary>
    public QuicTupleEndpointInfo? TupleLocal { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
