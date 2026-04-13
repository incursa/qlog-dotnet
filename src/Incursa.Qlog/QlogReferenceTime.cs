namespace Incursa.Qlog;

/// <summary>
/// Represents qlog reference-time metadata.
/// </summary>
public sealed class QlogReferenceTime
{
    /// <summary>
    /// Gets or sets the clock type.
    /// </summary>
    public string ClockType { get; set; } = QlogKnownValues.SystemClockType;

    /// <summary>
    /// Gets or sets the epoch string.
    /// </summary>
    public string Epoch { get; set; } = "1970-01-01T00:00:00.000Z";

    /// <summary>
    /// Gets or sets the optional approximate wall-clock time.
    /// </summary>
    public string? WallClockTime { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive round-tripping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
