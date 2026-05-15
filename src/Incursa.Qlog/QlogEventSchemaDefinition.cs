namespace Incursa.Qlog;

/// <summary>
/// Describes a qlog event schema URI and the authored event definitions it carries.
/// </summary>
public sealed class QlogEventSchemaDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QlogEventSchemaDefinition"/> class.
    /// </summary>
    /// <param name="schemaUri">The absolute event schema URI.</param>
    /// <param name="eventDefinitions">The authored event definitions belonging to the schema.</param>
    public QlogEventSchemaDefinition(Uri schemaUri, params QlogEventDefinition[] eventDefinitions)
    {
        ArgumentNullException.ThrowIfNull(schemaUri);

        if (!schemaUri.IsAbsoluteUri)
        {
            throw new ArgumentException("An event schema definition requires an absolute URI.", nameof(schemaUri));
        }

        ArgumentNullException.ThrowIfNull(eventDefinitions);

        foreach (QlogEventDefinition? eventDefinition in eventDefinitions)
        {
            if (eventDefinition is null)
            {
                throw new ArgumentException("Event definitions cannot contain null entries.", nameof(eventDefinitions));
            }
        }

        SchemaUri = schemaUri;
        EventDefinitions = eventDefinitions;
    }

    /// <summary>
    /// Gets the absolute event schema URI.
    /// </summary>
    public Uri SchemaUri { get; }

    /// <summary>
    /// Gets the authored event definitions defined by the schema.
    /// </summary>
    public IReadOnlyList<QlogEventDefinition> EventDefinitions { get; }
}
