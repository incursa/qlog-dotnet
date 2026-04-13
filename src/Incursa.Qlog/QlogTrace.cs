namespace Incursa.Qlog;

/// <summary>
/// Represents a contained qlog trace.
/// </summary>
public sealed class QlogTrace : QlogTraceComponent
{
    /// <summary>
    /// Gets or sets the optional trace title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the optional trace description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the shared trace fields.
    /// </summary>
    public QlogCommonFields? CommonFields { get; set; }

    /// <summary>
    /// Gets or sets the trace vantage point.
    /// </summary>
    public QlogVantagePoint? VantagePoint { get; set; }

    /// <summary>
    /// Gets the advisory event schema URIs associated with the trace.
    /// </summary>
    public IList<Uri> EventSchemas { get; } = new List<Uri>();

    /// <summary>
    /// Gets the events carried by the trace.
    /// </summary>
    public IList<QlogEvent> Events { get; } = new List<QlogEvent>();
}
