namespace Incursa.Qlog;

/// <summary>
/// Represents a qlog event in a contained trace.
/// </summary>
public sealed class QlogEvent
{
    /// <summary>
    /// Gets or sets the event timestamp.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Gets or sets the namespace-qualified event name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the event data object members.
    /// Use <see cref="QlogValue.Parse(string)"/>, <see cref="QlogValue.FromObject(IEnumerable{KeyValuePair{string, QlogValue}})"/>,
    /// and <see cref="QlogValue.FromArray(IEnumerable{QlogValue})"/> to build nested values without introducing a separate model type.
    /// </summary>
    public IDictionary<string, QlogValue> Data { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the optional tuple identifier.
    /// </summary>
    public string? Tuple { get; set; }

    /// <summary>
    /// Gets or sets the optional time format.
    /// </summary>
    public string? TimeFormat { get; set; }

    /// <summary>
    /// Gets or sets the optional group identifier.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the optional system-information object.
    /// </summary>
    public IDictionary<string, QlogValue>? SystemInfo { get; set; }

    /// <summary>
    /// Gets additional event members that should round-trip with the event.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
