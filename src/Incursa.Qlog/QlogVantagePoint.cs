namespace Incursa.Qlog;

/// <summary>
/// Represents qlog vantage-point metadata.
/// </summary>
public sealed class QlogVantagePoint
{
    /// <summary>
    /// Gets or sets the optional vantage-point name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the vantage-point type.
    /// </summary>
    public string Type { get; set; } = QlogKnownValues.UnknownVantagePoint;

    /// <summary>
    /// Gets or sets the optional flow direction.
    /// </summary>
    public string? Flow { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive round-tripping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
