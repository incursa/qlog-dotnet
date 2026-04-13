namespace Incursa.Qlog;

/// <summary>
/// Represents a contained qlog file envelope.
/// </summary>
public sealed class QlogFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QlogFile"/> class with contained JSON defaults.
    /// </summary>
    public QlogFile()
    {
        FileSchema = QlogKnownValues.ContainedFileSchemaUri;
        SerializationFormat = QlogKnownValues.ContainedJsonSerializationFormat;
    }

    /// <summary>
    /// Gets or sets the file schema URI.
    /// </summary>
    public Uri FileSchema { get; set; }

    /// <summary>
    /// Gets or sets the serialization format media type.
    /// </summary>
    public string SerializationFormat { get; set; }

    /// <summary>
    /// Gets or sets the optional file title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the optional file description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the contained traces and trace errors carried by the file.
    /// </summary>
    public IList<QlogTraceComponent> Traces { get; } = new List<QlogTraceComponent>();

    /// <summary>
    /// Gets additional members that should round-trip with the file envelope.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
