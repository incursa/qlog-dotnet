namespace Incursa.Qlog;

/// <summary>
/// Provides the draft-known qlog values used by the contained JSON baseline.
/// </summary>
public static class QlogKnownValues
{
    /// <summary>
    /// Gets the contained qlog file schema URI from the recorded draft baseline.
    /// </summary>
    public static Uri ContainedFileSchemaUri { get; } = new("urn:ietf:params:qlog:file:contained", UriKind.Absolute);

    /// <summary>
    /// Gets the default media type for contained qlog JSON artifacts.
    /// </summary>
    public const string ContainedJsonSerializationFormat = "application/qlog+json";

    /// <summary>
    /// Gets the qlog time format for timestamps measured relative to the reference epoch.
    /// </summary>
    public const string RelativeToEpochTimeFormat = "relative_to_epoch";

    /// <summary>
    /// Gets the qlog time format for timestamps measured relative to the previous event.
    /// </summary>
    public const string RelativeToPreviousEventTimeFormat = "relative_to_previous_event";

    /// <summary>
    /// Gets the qlog clock type for system time.
    /// </summary>
    public const string SystemClockType = "system";

    /// <summary>
    /// Gets the qlog clock type for monotonic time.
    /// </summary>
    public const string MonotonicClockType = "monotonic";

    /// <summary>
    /// Gets the qlog sentinel value for an unknown epoch.
    /// </summary>
    public const string UnknownEpoch = "unknown";

    /// <summary>
    /// Gets the client vantage-point identifier.
    /// </summary>
    public const string ClientVantagePoint = "client";

    /// <summary>
    /// Gets the server vantage-point identifier.
    /// </summary>
    public const string ServerVantagePoint = "server";

    /// <summary>
    /// Gets the network vantage-point identifier.
    /// </summary>
    public const string NetworkVantagePoint = "network";

    /// <summary>
    /// Gets the unknown vantage-point identifier.
    /// </summary>
    public const string UnknownVantagePoint = "unknown";
}
