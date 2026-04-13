namespace Incursa.Qlog.Quic;

/// <summary>
/// Represents the payload for <c>quic:connection_id_updated</c>.
/// </summary>
public sealed class QuicConnectionIdUpdated
{
    /// <summary>
    /// Gets or sets the endpoint that applied the new connection ID.
    /// </summary>
    public string? Initiator { get; set; }

    /// <summary>
    /// Gets or sets the previous connection ID, if known.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new connection ID, if known.
    /// </summary>
    public string? New { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:migration_state_updated</c>.
/// </summary>
public sealed class QuicMigrationStateUpdated
{
    /// <summary>
    /// Gets or sets the previous migration state, if known.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new migration state.
    /// </summary>
    public string New { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tuple identifier associated with this migration step, if known.
    /// </summary>
    public string? TupleId { get; set; }

    /// <summary>
    /// Gets or sets the tuple information for traffic going toward the remote endpoint, if known.
    /// </summary>
    public QuicTupleEndpointInfo? TupleRemote { get; set; }

    /// <summary>
    /// Gets or sets the tuple information for traffic arriving at the local endpoint, if known.
    /// </summary>
    public QuicTupleEndpointInfo? TupleLocal { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:key_updated</c>.
/// </summary>
public sealed class QuicKeyUpdated
{
    /// <summary>
    /// Gets or sets the qlog key type.
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous key bytes as a hex string, if captured.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new key bytes as a hex string, if captured.
    /// </summary>
    public string? New { get; set; }

    /// <summary>
    /// Gets or sets the 1-RTT key phase, if applicable.
    /// </summary>
    public ulong? KeyPhase { get; set; }

    /// <summary>
    /// Gets or sets the key update trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:key_discarded</c>.
/// </summary>
public sealed class QuicKeyDiscarded
{
    /// <summary>
    /// Gets or sets the qlog key type.
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the discarded key bytes as a hex string, if captured.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the 1-RTT key phase, if applicable.
    /// </summary>
    public ulong? KeyPhase { get; set; }

    /// <summary>
    /// Gets or sets the key discard trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:recovery_parameters_set</c>.
/// </summary>
public sealed class QuicRecoveryParametersSet
{
    /// <summary>
    /// Gets or sets the packet reordering threshold, if known.
    /// </summary>
    public ushort? ReorderingThreshold { get; set; }

    /// <summary>
    /// Gets or sets the time threshold multiplier, if known.
    /// </summary>
    public double? TimeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the timer granularity in milliseconds.
    /// </summary>
    public ushort? TimerGranularity { get; set; }

    /// <summary>
    /// Gets or sets the initial RTT in milliseconds, if known.
    /// </summary>
    public double? InitialRtt { get; set; }

    /// <summary>
    /// Gets or sets the maximum datagram size in bytes, if known.
    /// </summary>
    public uint? MaxDatagramSize { get; set; }

    /// <summary>
    /// Gets or sets the initial congestion window in bytes, if known.
    /// </summary>
    public ulong? InitialCongestionWindow { get; set; }

    /// <summary>
    /// Gets or sets the minimum congestion window in bytes, if known.
    /// </summary>
    public ulong? MinimumCongestionWindow { get; set; }

    /// <summary>
    /// Gets or sets the loss reduction factor, if known.
    /// </summary>
    public double? LossReductionFactor { get; set; }

    /// <summary>
    /// Gets or sets the persistent congestion threshold, if known.
    /// </summary>
    public ushort? PersistentCongestionThreshold { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:recovery_metrics_updated</c>.
/// </summary>
public sealed class QuicRecoveryMetricsUpdated
{
    /// <summary>
    /// Gets or sets the minimum RTT in milliseconds, if updated.
    /// </summary>
    public double? MinRtt { get; set; }

    /// <summary>
    /// Gets or sets the smoothed RTT in milliseconds, if updated.
    /// </summary>
    public double? SmoothedRtt { get; set; }

    /// <summary>
    /// Gets or sets the latest RTT in milliseconds, if updated.
    /// </summary>
    public double? LatestRtt { get; set; }

    /// <summary>
    /// Gets or sets the RTT variance in milliseconds, if updated.
    /// </summary>
    public double? RttVariance { get; set; }

    /// <summary>
    /// Gets or sets the current PTO count, if updated.
    /// </summary>
    public ushort? PtoCount { get; set; }

    /// <summary>
    /// Gets or sets the congestion window in bytes, if updated.
    /// </summary>
    public ulong? CongestionWindow { get; set; }

    /// <summary>
    /// Gets or sets the bytes currently in flight, if updated.
    /// </summary>
    public ulong? BytesInFlight { get; set; }

    /// <summary>
    /// Gets or sets the slow-start threshold in bytes, if updated.
    /// </summary>
    public ulong? Ssthresh { get; set; }

    /// <summary>
    /// Gets or sets the packet count currently in flight, if updated.
    /// </summary>
    public ulong? PacketsInFlight { get; set; }

    /// <summary>
    /// Gets or sets the pacing rate in bits per second, if updated.
    /// </summary>
    public ulong? PacingRate { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:congestion_state_updated</c>.
/// </summary>
public sealed class QuicCongestionStateUpdated
{
    /// <summary>
    /// Gets or sets the previous congestion state, if known.
    /// </summary>
    public string? Old { get; set; }

    /// <summary>
    /// Gets or sets the new congestion state.
    /// </summary>
    public string New { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trigger that caused the state transition, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the payload for <c>quic:packet_lost</c>.
/// </summary>
public sealed class QuicPacketLost
{
    /// <summary>
    /// Gets or sets the best-effort packet header metadata, if tracked.
    /// </summary>
    public QuicPacketHeader? Header { get; set; }

    /// <summary>
    /// Gets logged frame representations without forcing a full frame hierarchy for this slice.
    /// </summary>
    public IList<QlogValue> Frames { get; } = new List<QlogValue>();

    /// <summary>
    /// Gets or sets a value indicating whether the lost packet was an MTU probe.
    /// </summary>
    public bool? IsMtuProbePacket { get; set; }

    /// <summary>
    /// Gets or sets the loss trigger, if known.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive mapping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
