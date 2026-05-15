using Incursa.Qlog;

namespace Incursa.Qlog.Quic;

/// <summary>
/// Provides qlog event schema metadata for the recorded QUIC draft event namespace.
/// </summary>
public static class QlogQuicKnownEventSchemas
{
    /// <summary>
    /// Gets the draft QUIC event schema metadata used by this package.
    /// </summary>
    public static QlogEventSchemaDefinition Draft { get; } = new(
        QlogQuicKnownValues.DraftEventSchemaUri,
        new QlogEventDefinition(QlogQuicKnownValues.ServerListeningEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.ConnectionStartedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.ConnectionClosedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.ConnectionIdUpdatedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.ConnectionStateUpdatedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.TupleAssignedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.VersionInformationEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.AlpnInformationEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.ParametersSetEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.ParametersRestoredEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.PacketSentEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.PacketReceivedEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.PacketDroppedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.PacketBufferedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.PacketsAckedEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.UdpDatagramsSentEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.UdpDatagramsReceivedEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.UdpDatagramDroppedEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.StreamStateUpdatedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.StreamDataMovedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.DatagramDataMovedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.MigrationStateUpdatedEventName, QlogEventImportanceLevel.Extra),
        new QlogEventDefinition(QlogQuicKnownValues.KeyUpdatedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.KeyDiscardedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.RecoveryParametersSetEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.RecoveryMetricsUpdatedEventName, QlogEventImportanceLevel.Core),
        new QlogEventDefinition(QlogQuicKnownValues.CongestionStateUpdatedEventName, QlogEventImportanceLevel.Base),
        new QlogEventDefinition(QlogQuicKnownValues.PacketLostEventName, QlogEventImportanceLevel.Core));
}
