namespace Incursa.Qlog.Quic;

/// <summary>
/// Represents one half of a QUIC path tuple.
/// </summary>
public sealed class QuicTupleEndpointInfo
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
    /// Gets the connection IDs associated with this tuple half.
    /// </summary>
    public IList<string> ConnectionIds { get; } = new List<string>();

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
