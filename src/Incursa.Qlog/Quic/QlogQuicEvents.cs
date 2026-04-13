using System.Text.Json;

namespace Incursa.Qlog.Quic;

/// <summary>
/// Creates generic qlog events from the bounded QUIC vocabulary payloads.
/// </summary>
public static class QlogQuicEvents
{
    /// <summary>
    /// Ensures that the trace advertises the draft QUIC event schema URI required by the current baseline.
    /// </summary>
    /// <param name="trace">The target trace.</param>
    public static void RegisterDraftSchema(QlogTrace trace)
    {
        ArgumentNullException.ThrowIfNull(trace);

        if (!trace.EventSchemas.Contains(QlogQuicKnownValues.DraftEventSchemaUri))
        {
            trace.EventSchemas.Add(QlogQuicKnownValues.DraftEventSchemaUri);
        }
    }

    /// <summary>
    /// Creates a <c>quic:server_listening</c> event.
    /// </summary>
    public static QlogEvent CreateServerListening(double time, QuicServerListening data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return CreateEvent(time, QlogQuicKnownValues.ServerListeningEventName, target =>
        {
            AddOptionalString(target, "ip_v4", data.IpV4);
            AddOptionalNumber(target, "port_v4", data.PortV4);
            AddOptionalString(target, "ip_v6", data.IpV6);
            AddOptionalNumber(target, "port_v6", data.PortV6);
            AddOptionalBoolean(target, "retry_required", data.RetryRequired);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:connection_started</c> event.
    /// </summary>
    public static QlogEvent CreateConnectionStarted(double time, QuicConnectionStarted data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Local);
        ArgumentNullException.ThrowIfNull(data.Remote);

        return CreateEvent(time, QlogQuicKnownValues.ConnectionStartedEventName, target =>
        {
            target["local"] = CreateTupleEndpointInfoValue(data.Local);
            target["remote"] = CreateTupleEndpointInfoValue(data.Remote);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:connection_closed</c> event.
    /// </summary>
    public static QlogEvent CreateConnectionClosed(double time, QuicConnectionClosed data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateInitiator(data.Initiator);

        return CreateEvent(time, QlogQuicKnownValues.ConnectionClosedEventName, target =>
        {
            AddOptionalString(target, "initiator", data.Initiator);
            AddOptionalString(target, "connection_error", data.ConnectionError);
            AddOptionalString(target, "application_error", data.ApplicationError);
            AddOptionalNumber(target, "error_code", data.ErrorCode);
            AddOptionalNumber(target, "internal_code", data.InternalCode);
            AddOptionalString(target, "reason", data.Reason);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:connection_state_updated</c> event.
    /// </summary>
    public static QlogEvent CreateConnectionStateUpdated(double time, QuicConnectionStateUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRequiredString(data.New, nameof(data.New));

        return CreateEvent(time, QlogQuicKnownValues.ConnectionStateUpdatedEventName, target =>
        {
            AddOptionalString(target, "old", data.Old);
            target["new"] = QlogValue.FromString(data.New);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:tuple_assigned</c> event.
    /// </summary>
    public static QlogEvent CreateTupleAssigned(double time, QuicTupleAssigned data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateTupleId(data.TupleId, nameof(data.TupleId));

        return CreateEvent(time, QlogQuicKnownValues.TupleAssignedEventName, target =>
        {
            target["tuple_id"] = QlogValue.FromString(data.TupleId);

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
    /// Creates a <c>quic:version_information</c> event.
    /// </summary>
    public static QlogEvent CreateVersionInformation(double time, QuicVersionInformation data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateNonEmptyValues(data.ServerVersions, nameof(data.ServerVersions));
        ValidateNonEmptyValues(data.ClientVersions, nameof(data.ClientVersions));

        return CreateEvent(time, QlogQuicKnownValues.VersionInformationEventName, target =>
        {
            AddOptionalStringArray(target, "server_versions", data.ServerVersions);
            AddOptionalStringArray(target, "client_versions", data.ClientVersions);
            AddOptionalString(target, "chosen_version", data.ChosenVersion);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:alpn_information</c> event.
    /// </summary>
    public static QlogEvent CreateAlpnInformation(double time, QuicAlpnInformation data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return CreateEvent(time, QlogQuicKnownValues.AlpnInformationEventName, target =>
        {
            AddOptionalAlpnArray(target, "server_alpns", data.ServerAlpns);
            AddOptionalAlpnArray(target, "client_alpns", data.ClientAlpns);

            if (data.ChosenAlpn is not null)
            {
                target["chosen_alpn"] = CreateAlpnIdentifierValue(data.ChosenAlpn);
            }

            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:parameters_set</c> event.
    /// </summary>
    public static QlogEvent CreateParametersSet(double time, QuicParametersSet data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateInitiator(data.Initiator);

        return CreateEvent(time, QlogQuicKnownValues.ParametersSetEventName, target =>
        {
            AddOptionalString(target, "initiator", data.Initiator);
            AddOptionalBoolean(target, "resumption_allowed", data.ResumptionAllowed);
            AddOptionalBoolean(target, "early_data_enabled", data.EarlyDataEnabled);
            AddOptionalString(target, "tls_cipher", data.TlsCipher);
            AddOptionalString(target, "original_destination_connection_id", data.OriginalDestinationConnectionId);
            AddOptionalString(target, "initial_source_connection_id", data.InitialSourceConnectionId);
            AddOptionalString(target, "retry_source_connection_id", data.RetrySourceConnectionId);
            AddOptionalString(target, "stateless_reset_token", data.StatelessResetToken);
            AddOptionalBoolean(target, "disable_active_migration", data.DisableActiveMigration);
            AddOptionalNumber(target, "max_idle_timeout", data.MaxIdleTimeout);
            AddOptionalNumber(target, "max_udp_payload_size", data.MaxUdpPayloadSize);
            AddOptionalNumber(target, "ack_delay_exponent", data.AckDelayExponent);
            AddOptionalNumber(target, "max_ack_delay", data.MaxAckDelay);
            AddOptionalNumber(target, "active_connection_id_limit", data.ActiveConnectionIdLimit);
            AddOptionalNumber(target, "initial_max_data", data.InitialMaxData);
            AddOptionalNumber(target, "initial_max_stream_data_bidi_local", data.InitialMaxStreamDataBidiLocal);
            AddOptionalNumber(target, "initial_max_stream_data_bidi_remote", data.InitialMaxStreamDataBidiRemote);
            AddOptionalNumber(target, "initial_max_stream_data_uni", data.InitialMaxStreamDataUni);
            AddOptionalNumber(target, "initial_max_streams_bidi", data.InitialMaxStreamsBidi);
            AddOptionalNumber(target, "initial_max_streams_uni", data.InitialMaxStreamsUni);

            if (data.PreferredAddress is not null)
            {
                target["preferred_address"] = CreatePreferredAddressValue(data.PreferredAddress);
            }

            AddOptionalUnknownParameters(target, data.UnknownParameters);
            AddOptionalNumber(target, "max_datagram_frame_size", data.MaxDatagramFrameSize);
            AddOptionalBoolean(target, "grease_quic_bit", data.GreaseQuicBit);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:parameters_restored</c> event.
    /// </summary>
    public static QlogEvent CreateParametersRestored(double time, QuicParametersRestored data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return CreateEvent(time, QlogQuicKnownValues.ParametersRestoredEventName, target =>
        {
            AddOptionalBoolean(target, "disable_active_migration", data.DisableActiveMigration);
            AddOptionalNumber(target, "max_idle_timeout", data.MaxIdleTimeout);
            AddOptionalNumber(target, "max_udp_payload_size", data.MaxUdpPayloadSize);
            AddOptionalNumber(target, "active_connection_id_limit", data.ActiveConnectionIdLimit);
            AddOptionalNumber(target, "initial_max_data", data.InitialMaxData);
            AddOptionalNumber(target, "initial_max_stream_data_bidi_local", data.InitialMaxStreamDataBidiLocal);
            AddOptionalNumber(target, "initial_max_stream_data_bidi_remote", data.InitialMaxStreamDataBidiRemote);
            AddOptionalNumber(target, "initial_max_stream_data_uni", data.InitialMaxStreamDataUni);
            AddOptionalNumber(target, "initial_max_streams_bidi", data.InitialMaxStreamsBidi);
            AddOptionalNumber(target, "initial_max_streams_uni", data.InitialMaxStreamsUni);
            AddOptionalNumber(target, "max_datagram_frame_size", data.MaxDatagramFrameSize);
            AddOptionalBoolean(target, "grease_quic_bit", data.GreaseQuicBit);
            AddExtensions(target, data.ExtensionData);
        });
    }

    private static QlogEvent CreateEvent(double time, string eventName, Action<IDictionary<string, QlogValue>> populateData)
    {
        QlogEvent qlogEvent = new()
        {
            Time = time,
            Name = eventName,
        };

        populateData(qlogEvent.Data);
        return qlogEvent;
    }

    private static void AddOptionalString(IDictionary<string, QlogValue> target, string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[propertyName] = QlogValue.FromString(value);
        }
    }

    private static void AddOptionalBoolean(IDictionary<string, QlogValue> target, string propertyName, bool? value)
    {
        if (value.HasValue)
        {
            target[propertyName] = QlogValue.FromBoolean(value.Value);
        }
    }

    private static void AddOptionalNumber(IDictionary<string, QlogValue> target, string propertyName, ushort? value)
    {
        if (value.HasValue)
        {
            target[propertyName] = QlogValue.FromNumber((long)value.Value);
        }
    }

    private static void AddOptionalNumber(IDictionary<string, QlogValue> target, string propertyName, ulong? value)
    {
        if (value.HasValue)
        {
            target[propertyName] = QlogValue.FromNumber(value.Value);
        }
    }

    private static void AddOptionalStringArray(IDictionary<string, QlogValue> target, string propertyName, IList<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, QlogValueFactory.String);
    }

    private static void AddOptionalAlpnArray(IDictionary<string, QlogValue> target, string propertyName, IList<QuicAlpnIdentifier> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, CreateAlpnIdentifierValue);
    }

    private static void AddOptionalUnknownParameters(IDictionary<string, QlogValue> target, IList<QuicUnknownParameter> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target["unknown_parameters"] = QlogValueFactory.Array(values, CreateUnknownParameterValue);
    }

    private static void AddExtensions(IDictionary<string, QlogValue> target, IDictionary<string, QlogValue> extensionData)
    {
        foreach (KeyValuePair<string, QlogValue> entry in extensionData)
        {
            target[entry.Key] = entry.Value;
        }
    }

    private static QlogValue CreateTupleEndpointInfoValue(QuicTupleEndpointInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateNonEmptyValues(value.ConnectionIds, nameof(value.ConnectionIds));

        return QlogValueFactory.Object(writer =>
        {
            if (!string.IsNullOrWhiteSpace(value.IpV4))
            {
                writer.WriteString("ip_v4", value.IpV4);
            }

            if (value.PortV4.HasValue)
            {
                writer.WriteNumber("port_v4", value.PortV4.Value);
            }

            if (!string.IsNullOrWhiteSpace(value.IpV6))
            {
                writer.WriteString("ip_v6", value.IpV6);
            }

            if (value.PortV6.HasValue)
            {
                writer.WriteNumber("port_v6", value.PortV6.Value);
            }

            if (value.ConnectionIds.Count > 0)
            {
                writer.WritePropertyName("connection_ids");
                writer.WriteStartArray();
                foreach (string connectionId in value.ConnectionIds)
                {
                    ValidateRequiredString(connectionId, nameof(value.ConnectionIds));
                    writer.WriteStringValue(connectionId);
                }

                writer.WriteEndArray();
            }

            WriteExtensions(writer, value.ExtensionData);
        });
    }

    private static QlogValue CreateAlpnIdentifierValue(QuicAlpnIdentifier value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return QlogValueFactory.Object(writer => WriteAlpnIdentifierProperties(writer, value));
    }

    private static QlogValue CreateUnknownParameterValue(QuicUnknownParameter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return QlogValueFactory.Object(writer =>
        {
            writer.WriteNumber("id", value.Id);
            if (!string.IsNullOrWhiteSpace(value.Value))
            {
                writer.WriteString("value", value.Value);
            }
        });
    }

    private static void WriteAlpnIdentifierProperties(Utf8JsonWriter writer, QuicAlpnIdentifier value)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value.ByteValue) && string.IsNullOrWhiteSpace(value.StringValue))
        {
            throw new InvalidOperationException("ALPN identifiers must define at least a byte_value or string_value.");
        }

        if (!string.IsNullOrWhiteSpace(value.ByteValue))
        {
            writer.WriteString("byte_value", value.ByteValue);
        }

        if (!string.IsNullOrWhiteSpace(value.StringValue))
        {
            writer.WriteString("string_value", value.StringValue);
        }
    }

    private static QlogValue CreatePreferredAddressValue(QuicPreferredAddress value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateRequiredString(value.ConnectionId, nameof(value.ConnectionId));
        ValidateRequiredString(value.StatelessResetToken, nameof(value.StatelessResetToken));

        return QlogValueFactory.Object(writer =>
        {
            if (!string.IsNullOrWhiteSpace(value.IpV4))
            {
                writer.WriteString("ip_v4", value.IpV4);
            }

            if (value.PortV4.HasValue)
            {
                writer.WriteNumber("port_v4", value.PortV4.Value);
            }

            if (!string.IsNullOrWhiteSpace(value.IpV6))
            {
                writer.WriteString("ip_v6", value.IpV6);
            }

            if (value.PortV6.HasValue)
            {
                writer.WriteNumber("port_v6", value.PortV6.Value);
            }

            writer.WriteString("connection_id", value.ConnectionId);
            writer.WriteString("stateless_reset_token", value.StatelessResetToken);
        });
    }

    private static void WriteExtensions(Utf8JsonWriter writer, IDictionary<string, QlogValue> extensionData)
    {
        foreach (KeyValuePair<string, QlogValue> entry in extensionData)
        {
            writer.WritePropertyName(entry.Key);
            entry.Value.WriteTo(writer);
        }
    }

    private static void ValidateInitiator(string? initiator)
    {
        if (initiator is null)
        {
            return;
        }

        if (!string.Equals(initiator, QlogQuicKnownValues.LocalInitiator, StringComparison.Ordinal) &&
            !string.Equals(initiator, QlogQuicKnownValues.RemoteInitiator, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Initiator must be '{QlogQuicKnownValues.LocalInitiator}' or '{QlogQuicKnownValues.RemoteInitiator}'.");
        }
    }

    private static void ValidateRequiredString(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"'{parameterName}' requires a non-empty value.");
        }
    }

    private static void ValidateTupleId(string? value, string parameterName)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"'{parameterName}' requires a value.");
        }

        if (value.Length > 0 && string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"'{parameterName}' cannot be whitespace.");
        }
    }

    private static void ValidateNonEmptyValues(IList<string> values, string parameterName)
    {
        if (values.Count == 0)
        {
            return;
        }

        foreach (string value in values)
        {
            ValidateRequiredString(value, parameterName);
        }
    }
}
