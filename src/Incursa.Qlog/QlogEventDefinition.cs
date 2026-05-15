namespace Incursa.Qlog;

/// <summary>
/// Describes a single authored qlog event definition and its importance metadata.
/// </summary>
public sealed class QlogEventDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QlogEventDefinition"/> class.
    /// </summary>
    /// <param name="name">The fully qualified qlog event name.</param>
    /// <param name="importanceLevel">The importance metadata associated with the event.</param>
    public QlogEventDefinition(string name, QlogEventImportanceLevel importanceLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("An event definition requires a non-empty name.", nameof(name));
        }

        Name = name;
        ImportanceLevel = importanceLevel;
    }

    /// <summary>
    /// Gets the fully qualified qlog event name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the event importance metadata.
    /// </summary>
    public QlogEventImportanceLevel ImportanceLevel { get; }
}
