namespace Incursa.Qlog;

/// <summary>
/// Represents an entry in a contained qlog file's <c>traces</c> array.
/// </summary>
public abstract class QlogTraceComponent
{
    /// <summary>
    /// Gets additional members that should round-trip with the component.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
