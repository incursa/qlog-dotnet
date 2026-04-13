namespace Incursa.Qlog.Quic;

public static partial class QlogQuicEvents
{
    private static readonly HashSet<string> MigrationStates = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.MigrationStateProbingStarted,
        QlogQuicKnownValues.MigrationStateProbingAbandoned,
        QlogQuicKnownValues.MigrationStateProbingSuccessful,
        QlogQuicKnownValues.MigrationStateStarted,
        QlogQuicKnownValues.MigrationStateAbandoned,
        QlogQuicKnownValues.MigrationStateComplete,
    };

    private static readonly HashSet<string> KeyTypes = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.KeyTypeServerInitialSecret,
        QlogQuicKnownValues.KeyTypeClientInitialSecret,
        QlogQuicKnownValues.KeyTypeServerHandshakeSecret,
        QlogQuicKnownValues.KeyTypeClientHandshakeSecret,
        QlogQuicKnownValues.KeyTypeServerZeroRttSecret,
        QlogQuicKnownValues.KeyTypeClientZeroRttSecret,
        QlogQuicKnownValues.KeyTypeServerOneRttSecret,
        QlogQuicKnownValues.KeyTypeClientOneRttSecret,
    };

    private static readonly HashSet<string> KeyLifecycleTriggers = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.KeyLifecycleTriggerTls,
        QlogQuicKnownValues.KeyLifecycleTriggerRemoteUpdate,
        QlogQuicKnownValues.KeyLifecycleTriggerLocalUpdate,
    };

    private static readonly HashSet<string> PacketLostTriggers = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.PacketLostTriggerReorderingThreshold,
        QlogQuicKnownValues.PacketLostTriggerTimeThreshold,
        QlogQuicKnownValues.PacketLostTriggerPtoExpired,
    };

    /// <summary>
    /// Creates a <c>quic:connection_id_updated</c> event.
    /// </summary>
    public static QlogEvent CreateConnectionIdUpdated(double time, QuicConnectionIdUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRequiredInitiator(data.Initiator, nameof(data.Initiator));

        return CreateEvent(time, QlogQuicKnownValues.ConnectionIdUpdatedEventName, target =>
        {
            target["initiator"] = QlogValue.FromString(data.Initiator!);
            AddOptionalString(target, "old", data.Old);
            AddOptionalString(target, "new", data.New);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:migration_state_updated</c> event.
    /// </summary>
    public static QlogEvent CreateMigrationStateUpdated(double time, QuicMigrationStateUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalKnownValue(data.Old, nameof(data.Old), MigrationStates);
        ValidateKnownValue(data.New, nameof(data.New), MigrationStates);

        if (data.TupleId is not null)
        {
            ValidateTupleId(data.TupleId, nameof(data.TupleId));
        }

        return CreateEvent(time, QlogQuicKnownValues.MigrationStateUpdatedEventName, target =>
        {
            AddOptionalString(target, "old", data.Old);
            target["new"] = QlogValue.FromString(data.New);

            if (data.TupleId is not null)
            {
                target["tuple_id"] = QlogValue.FromString(data.TupleId);
            }

            if (data.TupleRemote is not null)
            {
                target["tuple_remote"] = CreateTupleEndpointInfoValue(data.TupleRemote);
            }

            if (data.TupleLocal is not null)
            {
                target["tuple_local"] = CreateTupleEndpointInfoValue(data.TupleLocal);
            }

            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:key_updated</c> event.
    /// </summary>
    public static QlogEvent CreateKeyUpdated(double time, QuicKeyUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateKnownValue(data.KeyType, nameof(data.KeyType), KeyTypes);
        ValidateOptionalKnownValue(data.Trigger, nameof(data.Trigger), KeyLifecycleTriggers);

        return CreateEvent(time, QlogQuicKnownValues.KeyUpdatedEventName, target =>
        {
            target["key_type"] = QlogValue.FromString(data.KeyType);
            AddOptionalString(target, "old", data.Old);
            AddOptionalString(target, "new", data.New);
            AddOptionalNumber(target, "key_phase", data.KeyPhase);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:key_discarded</c> event.
    /// </summary>
    public static QlogEvent CreateKeyDiscarded(double time, QuicKeyDiscarded data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateKnownValue(data.KeyType, nameof(data.KeyType), KeyTypes);
        ValidateOptionalKnownValue(data.Trigger, nameof(data.Trigger), KeyLifecycleTriggers);

        return CreateEvent(time, QlogQuicKnownValues.KeyDiscardedEventName, target =>
        {
            target["key_type"] = QlogValue.FromString(data.KeyType);
            AddOptionalString(target, "key", data.Key);
            AddOptionalNumber(target, "key_phase", data.KeyPhase);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:recovery_parameters_set</c> event.
    /// </summary>
    public static QlogEvent CreateRecoveryParametersSet(double time, QuicRecoveryParametersSet data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!data.TimerGranularity.HasValue)
        {
            throw new InvalidOperationException($"'{nameof(data.TimerGranularity)}' requires a value.");
        }

        return CreateEvent(time, QlogQuicKnownValues.RecoveryParametersSetEventName, target =>
        {
            AddOptionalNumber(target, "reordering_threshold", data.ReorderingThreshold);
            AddOptionalNumber(target, "time_threshold", data.TimeThreshold);
            target["timer_granularity"] = QlogValue.FromNumber((long)data.TimerGranularity.Value);
            AddOptionalNumber(target, "initial_rtt", data.InitialRtt);
            AddOptionalNumber(target, "max_datagram_size", data.MaxDatagramSize);
            AddOptionalNumber(target, "initial_congestion_window", data.InitialCongestionWindow);
            AddOptionalNumber(target, "minimum_congestion_window", data.MinimumCongestionWindow);
            AddOptionalNumber(target, "loss_reduction_factor", data.LossReductionFactor);
            AddOptionalNumber(target, "persistent_congestion_threshold", data.PersistentCongestionThreshold);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:recovery_metrics_updated</c> event.
    /// </summary>
    public static QlogEvent CreateRecoveryMetricsUpdated(double time, QuicRecoveryMetricsUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRecoveryMetricsPayload(data);

        return CreateEvent(time, QlogQuicKnownValues.RecoveryMetricsUpdatedEventName, target =>
        {
            AddOptionalNumber(target, "min_rtt", data.MinRtt);
            AddOptionalNumber(target, "smoothed_rtt", data.SmoothedRtt);
            AddOptionalNumber(target, "latest_rtt", data.LatestRtt);
            AddOptionalNumber(target, "rtt_variance", data.RttVariance);
            AddOptionalNumber(target, "pto_count", data.PtoCount);
            AddOptionalNumber(target, "congestion_window", data.CongestionWindow);
            AddOptionalNumber(target, "bytes_in_flight", data.BytesInFlight);
            AddOptionalNumber(target, "ssthresh", data.Ssthresh);
            AddOptionalNumber(target, "packets_in_flight", data.PacketsInFlight);
            AddOptionalNumber(target, "pacing_rate", data.PacingRate);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:congestion_state_updated</c> event.
    /// </summary>
    public static QlogEvent CreateCongestionStateUpdated(double time, QuicCongestionStateUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRequiredString(data.New, nameof(data.New));

        return CreateEvent(time, QlogQuicKnownValues.CongestionStateUpdatedEventName, target =>
        {
            AddOptionalString(target, "old", data.Old);
            target["new"] = QlogValue.FromString(data.New);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:packet_lost</c> event.
    /// </summary>
    public static QlogEvent CreatePacketLost(double time, QuicPacketLost data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalPacketLostHeader(data.Header);
        ValidateNonNullValues(data.Frames, nameof(data.Frames));
        ValidateOptionalKnownValue(data.Trigger, nameof(data.Trigger), PacketLostTriggers);

        return CreateEvent(time, QlogQuicKnownValues.PacketLostEventName, target =>
        {
            AddOptionalPacketHeader(target, "header", data.Header);
            AddOptionalValuesArray(target, "frames", data.Frames);
            AddOptionalBoolean(target, "is_mtu_probe_packet", data.IsMtuProbePacket);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    private static void ValidateRequiredInitiator(string? initiator, string parameterName)
    {
        ValidateRequiredString(initiator, parameterName);
        ValidateInitiator(initiator);
    }

    private static void ValidateOptionalPacketLostHeader(QuicPacketHeader? value)
    {
        if (value is null)
        {
            return;
        }

        ValidatePacketHeader(value);

        if (!value.PacketNumber.HasValue)
        {
            throw new InvalidOperationException("'Header.PacketNumber' requires a value for packet_lost.");
        }
    }

    private static void ValidateRecoveryMetricsPayload(QuicRecoveryMetricsUpdated value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.MinRtt.HasValue ||
            value.SmoothedRtt.HasValue ||
            value.LatestRtt.HasValue ||
            value.RttVariance.HasValue ||
            value.PtoCount.HasValue ||
            value.CongestionWindow.HasValue ||
            value.BytesInFlight.HasValue ||
            value.Ssthresh.HasValue ||
            value.PacketsInFlight.HasValue ||
            value.PacingRate.HasValue ||
            value.ExtensionData.Count > 0)
        {
            return;
        }

        throw new InvalidOperationException("Recovery metrics updates must carry at least one metric or extension field.");
    }
}
