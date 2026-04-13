namespace Incursa.Qlog.Quic;

/// <summary>
/// Represents the payload for <c>quic:version_information</c>.
/// </summary>
public sealed class QuicVersionInformation
{
    /// <summary>
    /// Gets the versions supported by the server endpoint.
    /// </summary>
    public IList<string> ServerVersions { get; } = new List<string>();

    /// <summary>
    /// Gets the versions supported by the client endpoint.
    /// </summary>
    public IList<string> ClientVersions { get; } = new List<string>();

    /// <summary>
    /// Gets or sets the chosen version, if any overlap exists.
    /// </summary>
    public string? ChosenVersion { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:alpn_information</c>.
/// </summary>
public sealed class QuicAlpnInformation
{
    /// <summary>
    /// Gets the ALPN identifiers supported by the server endpoint.
    /// </summary>
    public IList<QuicAlpnIdentifier> ServerAlpns { get; } = new List<QuicAlpnIdentifier>();

    /// <summary>
    /// Gets the ALPN identifiers supported by the client endpoint.
    /// </summary>
    public IList<QuicAlpnIdentifier> ClientAlpns { get; } = new List<QuicAlpnIdentifier>();

    /// <summary>
    /// Gets or sets the chosen ALPN identifier, if any.
    /// </summary>
    public QuicAlpnIdentifier? ChosenAlpn { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents a logged ALPN identifier.
/// </summary>
public sealed class QuicAlpnIdentifier
{
    /// <summary>
    /// Gets or sets the raw byte encoding as a hex string, if known.
    /// </summary>
    public string? ByteValue { get; set; }

    /// <summary>
    /// Gets or sets the UTF-8 string form, if known.
    /// </summary>
    public string? StringValue { get; set; }
}

/// <summary>
/// Represents the payload for <c>quic:parameters_set</c>.
/// </summary>
public sealed class QuicParametersSet
{
    /// <summary>
    /// Gets or sets the side whose effective parameters are being described, if known.
    /// </summary>
    public string? Initiator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether resumption is allowed.
    /// </summary>
    public bool? ResumptionAllowed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether early data is enabled.
    /// </summary>
    public bool? EarlyDataEnabled { get; set; }

    /// <summary>
    /// Gets or sets the TLS cipher suite name, if known.
    /// </summary>
    public string? TlsCipher { get; set; }

    /// <summary>
    /// Gets or sets the original destination connection ID, if known.
    /// </summary>
    public string? OriginalDestinationConnectionId { get; set; }

    /// <summary>
    /// Gets or sets the initial source connection ID, if known.
    /// </summary>
    public string? InitialSourceConnectionId { get; set; }

    /// <summary>
    /// Gets or sets the retry source connection ID, if known.
    /// </summary>
    public string? RetrySourceConnectionId { get; set; }

    /// <summary>
    /// Gets or sets the stateless reset token, if known.
    /// </summary>
    public string? StatelessResetToken { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether active migration is disabled.
    /// </summary>
    public bool? DisableActiveMigration { get; set; }

    /// <summary>
    /// Gets or sets the maximum idle timeout.
    /// </summary>
    public ulong? MaxIdleTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum UDP payload size.
    /// </summary>
    public ulong? MaxUdpPayloadSize { get; set; }

    /// <summary>
    /// Gets or sets the acknowledgment delay exponent.
    /// </summary>
    public ulong? AckDelayExponent { get; set; }

    /// <summary>
    /// Gets or sets the maximum acknowledgment delay.
    /// </summary>
    public ulong? MaxAckDelay { get; set; }

    /// <summary>
    /// Gets or sets the active connection ID limit.
    /// </summary>
    public ulong? ActiveConnectionIdLimit { get; set; }

    /// <summary>
    /// Gets or sets the initial connection-wide data limit.
    /// </summary>
    public ulong? InitialMaxData { get; set; }

    /// <summary>
    /// Gets or sets the initial local bidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataBidiLocal { get; set; }

    /// <summary>
    /// Gets or sets the initial remote bidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataBidiRemote { get; set; }

    /// <summary>
    /// Gets or sets the initial unidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataUni { get; set; }

    /// <summary>
    /// Gets or sets the initial bidirectional stream count limit.
    /// </summary>
    public ulong? InitialMaxStreamsBidi { get; set; }

    /// <summary>
    /// Gets or sets the initial unidirectional stream count limit.
    /// </summary>
    public ulong? InitialMaxStreamsUni { get; set; }

    /// <summary>
    /// Gets or sets the preferred address, if advertised.
    /// </summary>
    public QuicPreferredAddress? PreferredAddress { get; set; }

    /// <summary>
    /// Gets the unknown transport parameters, if any were observed.
    /// </summary>
    public IList<QuicUnknownParameter> UnknownParameters { get; } = new List<QuicUnknownParameter>();

    /// <summary>
    /// Gets or sets the maximum DATAGRAM frame size.
    /// </summary>
    public ulong? MaxDatagramFrameSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the QUIC bit is greased.
    /// </summary>
    public bool? GreaseQuicBit { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the preferred-address transport parameter.
/// </summary>
public sealed class QuicPreferredAddress
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
    /// Gets or sets the advertised connection ID.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stateless reset token.
    /// </summary>
    public string StatelessResetToken { get; set; } = string.Empty;
}

/// <summary>
/// Represents an unknown transport parameter.
/// </summary>
public sealed class QuicUnknownParameter
{
    /// <summary>
    /// Gets or sets the transport parameter identifier.
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// Gets or sets the raw transport parameter value as a hex string, if captured.
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// Represents the payload for <c>quic:parameters_restored</c>.
/// </summary>
public sealed class QuicParametersRestored
{
    /// <summary>
    /// Gets or sets a value indicating whether active migration is disabled.
    /// </summary>
    public bool? DisableActiveMigration { get; set; }

    /// <summary>
    /// Gets or sets the maximum idle timeout.
    /// </summary>
    public ulong? MaxIdleTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum UDP payload size.
    /// </summary>
    public ulong? MaxUdpPayloadSize { get; set; }

    /// <summary>
    /// Gets or sets the active connection ID limit.
    /// </summary>
    public ulong? ActiveConnectionIdLimit { get; set; }

    /// <summary>
    /// Gets or sets the initial connection-wide data limit.
    /// </summary>
    public ulong? InitialMaxData { get; set; }

    /// <summary>
    /// Gets or sets the initial local bidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataBidiLocal { get; set; }

    /// <summary>
    /// Gets or sets the initial remote bidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataBidiRemote { get; set; }

    /// <summary>
    /// Gets or sets the initial unidirectional stream data limit.
    /// </summary>
    public ulong? InitialMaxStreamDataUni { get; set; }

    /// <summary>
    /// Gets or sets the initial bidirectional stream count limit.
    /// </summary>
    public ulong? InitialMaxStreamsBidi { get; set; }

    /// <summary>
    /// Gets or sets the initial unidirectional stream count limit.
    /// </summary>
    public ulong? InitialMaxStreamsUni { get; set; }

    /// <summary>
    /// Gets or sets the maximum DATAGRAM frame size.
    /// </summary>
    public ulong? MaxDatagramFrameSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the QUIC bit is greased.
    /// </summary>
    public bool? GreaseQuicBit { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
