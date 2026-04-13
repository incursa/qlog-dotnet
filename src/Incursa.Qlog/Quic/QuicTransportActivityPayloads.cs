namespace Incursa.Qlog.Quic;

/// <summary>
/// Represents shared raw byte metadata for qlog entities.
/// </summary>
public sealed class QuicRawInfo
{
    /// <summary>
    /// Gets or sets the full byte length of the entity, including headers and trailers.
    /// </summary>
    public ulong? Length { get; set; }

    /// <summary>
    /// Gets or sets the payload byte length of the entity, excluding headers and trailers.
    /// </summary>
    public ulong? PayloadLength { get; set; }

    /// <summary>
    /// Gets or sets the full or truncated raw entity bytes as a hex string.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents a QUIC token carried in a packet header.
/// </summary>
public sealed class QuicPacketToken
{
    /// <summary>
    /// Gets or sets the token type, if known.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets token metadata that remains implementation specific.
    /// </summary>
    public IDictionary<string, QlogValue> Details { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the raw token bytes, if captured.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the bounded packet-header shape used by the transport-activity slice.
/// </summary>
public sealed class QuicPacketHeader
{
    /// <summary>
    /// Gets or sets the qlog packet type.
    /// </summary>
    public string PacketType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw packet type value when the packet type is unknown.
    /// </summary>
    public ulong? PacketTypeBytes { get; set; }

    /// <summary>
    /// Gets or sets the 1-RTT spin bit, if known.
    /// </summary>
    public bool? SpinBit { get; set; }

    /// <summary>
    /// Gets or sets the resolved key phase, if known.
    /// </summary>
    public ulong? KeyPhase { get; set; }

    /// <summary>
    /// Gets or sets the raw key phase bit when the resolved key phase is unavailable.
    /// </summary>
    public bool? KeyPhaseBit { get; set; }

    /// <summary>
    /// Gets or sets the packet number length, if logged.
    /// </summary>
    public byte? PacketNumberLength { get; set; }

    /// <summary>
    /// Gets or sets the packet number, if logged.
    /// </summary>
    public ulong? PacketNumber { get; set; }

    /// <summary>
    /// Gets or sets the retry or resumption token, if logged.
    /// </summary>
    public QuicPacketToken? Token { get; set; }

    /// <summary>
    /// Gets or sets the long-header payload length field, if logged.
    /// </summary>
    public ushort? Length { get; set; }

    /// <summary>
    /// Gets or sets the packet version, if logged.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the source connection ID length, if logged.
    /// </summary>
    public byte? Scil { get; set; }

    /// <summary>
    /// Gets or sets the destination connection ID length, if logged.
    /// </summary>
    public byte? Dcil { get; set; }

    /// <summary>
    /// Gets or sets the source connection ID, if logged.
    /// </summary>
    public string? Scid { get; set; }

    /// <summary>
    /// Gets or sets the destination connection ID, if logged.
    /// </summary>
    public string? Dcid { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packet_sent</c>.
/// </summary>
public sealed class QuicPacketSent
{
    /// <summary>
    /// Gets or sets the logged packet header.
    /// </summary>
    public QuicPacketHeader? Header { get; set; }

    /// <summary>
    /// Gets logged frame representations without forcing a full frame hierarchy for this slice.
    /// </summary>
    public IList<QlogValue> Frames { get; } = new List<QlogValue>();

    /// <summary>
    /// Gets or sets the stateless reset token, if applicable.
    /// </summary>
    public string? StatelessResetToken { get; set; }

    /// <summary>
    /// Gets the supported versions for version-negotiation packets.
    /// </summary>
    public IList<string> SupportedVersions { get; } = new List<string>();

    /// <summary>
    /// Gets or sets raw packet bytes, if captured.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets or sets the coalescing datagram identifier, if known.
    /// </summary>
    public ulong? DatagramId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the packet is an MTU probe.
    /// </summary>
    public bool? IsMtuProbePacket { get; set; }

    /// <summary>
    /// Gets or sets the send trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packet_received</c>.
/// </summary>
public sealed class QuicPacketReceived
{
    /// <summary>
    /// Gets or sets the logged packet header.
    /// </summary>
    public QuicPacketHeader? Header { get; set; }

    /// <summary>
    /// Gets logged frame representations without forcing a full frame hierarchy for this slice.
    /// </summary>
    public IList<QlogValue> Frames { get; } = new List<QlogValue>();

    /// <summary>
    /// Gets or sets the stateless reset token, if applicable.
    /// </summary>
    public string? StatelessResetToken { get; set; }

    /// <summary>
    /// Gets the supported versions for version-negotiation packets.
    /// </summary>
    public IList<string> SupportedVersions { get; } = new List<string>();

    /// <summary>
    /// Gets or sets raw packet bytes, if captured.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets or sets the coalescing datagram identifier, if known.
    /// </summary>
    public ulong? DatagramId { get; set; }

    /// <summary>
    /// Gets or sets the receive trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packet_dropped</c>.
/// </summary>
public sealed class QuicPacketDropped
{
    /// <summary>
    /// Gets or sets the best-effort packet header metadata, if available.
    /// </summary>
    public QuicPacketHeader? Header { get; set; }

    /// <summary>
    /// Gets or sets raw packet bytes, if captured.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets or sets the coalescing datagram identifier, if known.
    /// </summary>
    public ulong? DatagramId { get; set; }

    /// <summary>
    /// Gets implementation-specific drop details.
    /// </summary>
    public IDictionary<string, QlogValue> Details { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the general drop trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packet_buffered</c>.
/// </summary>
public sealed class QuicPacketBuffered
{
    /// <summary>
    /// Gets or sets the best-effort packet header metadata, if available.
    /// </summary>
    public QuicPacketHeader? Header { get; set; }

    /// <summary>
    /// Gets or sets raw packet bytes, if captured.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets or sets the coalescing datagram identifier, if known.
    /// </summary>
    public ulong? DatagramId { get; set; }

    /// <summary>
    /// Gets or sets the buffering trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packets_acked</c>.
/// </summary>
public sealed class QuicPacketsAcked
{
    /// <summary>
    /// Gets or sets the packet number space, if not the application-data default.
    /// </summary>
    public string? PacketNumberSpace { get; set; }

    /// <summary>
    /// Gets the acknowledged packet numbers.
    /// </summary>
    public IList<ulong> PacketNumbers { get; } = new List<ulong>();

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:udp_datagrams_sent</c>.
/// </summary>
public sealed class QuicUdpDatagramsSent
{
    /// <summary>
    /// Gets or sets the number of datagrams passed to the socket together.
    /// </summary>
    public ushort? Count { get; set; }

    /// <summary>
    /// Gets raw UDP payload metadata for the sent datagrams.
    /// </summary>
    public IList<QuicRawInfo> Raw { get; } = new List<QuicRawInfo>();

    /// <summary>
    /// Gets the ECN markings applied to the sent datagrams.
    /// </summary>
    public IList<string> Ecn { get; } = new List<string>();

    /// <summary>
    /// Gets the datagram identifiers used for packet coalescing correlation.
    /// </summary>
    public IList<ulong> DatagramIds { get; } = new List<ulong>();

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:udp_datagrams_received</c>.
/// </summary>
public sealed class QuicUdpDatagramsReceived
{
    /// <summary>
    /// Gets or sets the number of datagrams received together.
    /// </summary>
    public ushort? Count { get; set; }

    /// <summary>
    /// Gets raw UDP payload metadata for the received datagrams.
    /// </summary>
    public IList<QuicRawInfo> Raw { get; } = new List<QuicRawInfo>();

    /// <summary>
    /// Gets the ECN markings observed on the received datagrams.
    /// </summary>
    public IList<string> Ecn { get; } = new List<string>();

    /// <summary>
    /// Gets the datagram identifiers used for packet coalescing correlation.
    /// </summary>
    public IList<ulong> DatagramIds { get; } = new List<ulong>();

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:udp_datagram_dropped</c>.
/// </summary>
public sealed class QuicUdpDatagramDropped
{
    /// <summary>
    /// Gets or sets raw UDP payload metadata for the dropped datagram.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:stream_state_updated</c>.
/// </summary>
public sealed class QuicStreamStateUpdated
{
    /// <summary>
    /// Gets or sets the stream identifier.
    /// </summary>
    public ulong StreamId { get; set; }

    /// <summary>
    /// Gets or sets the stream type, if logged.
    /// </summary>
    public string? StreamType { get; set; }

    /// <summary>
    /// Gets or sets the previous stream state, if logged.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new stream state.
    /// </summary>
    public string New { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stream-side state machine that changed.
    /// </summary>
    public string StreamSide { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the side that triggered the state change, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:stream_data_moved</c>.
/// </summary>
public sealed class QuicStreamDataMoved
{
    /// <summary>
    /// Gets or sets the stream identifier, if logged.
    /// </summary>
    public ulong? StreamId { get; set; }

    /// <summary>
    /// Gets or sets the stream offset, if logged.
    /// </summary>
    public ulong? Offset { get; set; }

    /// <summary>
    /// Gets or sets the source layer, if logged.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the destination layer, if logged.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Gets or sets additional movement context, if logged.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Gets or sets the moved byte information, if logged.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:datagram_data_moved</c>.
/// </summary>
public sealed class QuicDatagramDataMoved
{
    /// <summary>
    /// Gets or sets the source layer, if logged.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the destination layer, if logged.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Gets or sets the moved byte information, if logged.
    /// </summary>
    public QuicRawInfo? Raw { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
