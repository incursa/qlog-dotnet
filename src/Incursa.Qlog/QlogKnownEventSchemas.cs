namespace Incursa.Qlog;

/// <summary>
/// Provides qlog event schema metadata for the recorded main-schema namespaces.
/// </summary>
public static class QlogKnownEventSchemas
{
    /// <summary>
    /// Gets the generic loglevel event schema metadata.
    /// </summary>
    public static QlogEventSchemaDefinition LogLevel { get; } = new(
        new Uri("urn:ietf:params:qlog:events:loglevel", UriKind.Absolute),
        new QlogEventDefinition("loglevel:error", QlogEventImportanceLevel.Core),
        new QlogEventDefinition("loglevel:warning", QlogEventImportanceLevel.Base),
        new QlogEventDefinition("loglevel:info", QlogEventImportanceLevel.Extra),
        new QlogEventDefinition("loglevel:debug", QlogEventImportanceLevel.Extra),
        new QlogEventDefinition("loglevel:verbose", QlogEventImportanceLevel.Extra));

    /// <summary>
    /// Gets the generic simulation event schema metadata.
    /// </summary>
    public static QlogEventSchemaDefinition Simulation { get; } = new(
        new Uri("urn:ietf:params:qlog:events:simulation", UriKind.Absolute),
        new QlogEventDefinition("simulation:scenario", QlogEventImportanceLevel.Extra),
        new QlogEventDefinition("simulation:marker", QlogEventImportanceLevel.Extra));
}
