using System.Text.Json;

namespace Incursa.Qlog.Quic;

public static partial class QlogQuicEvents
{
    private static readonly HashSet<string> PacketTypes = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.PacketTypeInitial,
        QlogQuicKnownValues.PacketTypeHandshake,
        QlogQuicKnownValues.PacketTypeZeroRtt,
        QlogQuicKnownValues.PacketTypeOneRtt,
        QlogQuicKnownValues.PacketTypeRetry,
        QlogQuicKnownValues.PacketTypeVersionNegotiation,
        QlogQuicKnownValues.PacketTypeStatelessReset,
        QlogQuicKnownValues.PacketTypeUnknown,
    };

    private static readonly HashSet<string> PacketNumberSpaces = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.PacketNumberSpaceInitial,
        QlogQuicKnownValues.PacketNumberSpaceHandshake,
        QlogQuicKnownValues.PacketNumberSpaceApplicationData,
    };

    private static readonly HashSet<string> StreamTypes = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.StreamTypeUnidirectional,
        QlogQuicKnownValues.StreamTypeBidirectional,
    };

    private static readonly HashSet<string> StreamSides = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.StreamSideSending,
        QlogQuicKnownValues.StreamSideReceiving,
    };

    private static readonly HashSet<string> StreamStates = new(StringComparer.Ordinal)
    {
        "idle",
        "open",
        "closed",
        "half_closed_local",
        "half_closed_remote",
        "ready",
        "send",
        "data_sent",
        "reset_sent",
        "reset_received",
        "receive",
        "size_known",
        "data_read",
        "reset_read",
        "data_received",
        "destroyed",
    };

    private static readonly HashSet<string> DataLocations = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.DataLocationApplication,
        QlogQuicKnownValues.DataLocationTransport,
        QlogQuicKnownValues.DataLocationNetwork,
    };

    private static readonly HashSet<string> DataMovedAdditionalInfo = new(StringComparer.Ordinal)
    {
        QlogQuicKnownValues.DataMovedAdditionalInfoFinSet,
        QlogQuicKnownValues.DataMovedAdditionalInfoStreamReset,
    };

    private static readonly HashSet<string> TokenTypes = new(StringComparer.Ordinal)
    {
        "retry",
        "resumption",
    };

    /// <summary>
    /// Creates a <c>quic:packet_sent</c> event.
    /// </summary>
    public static QlogEvent CreatePacketSent(double time, QuicPacketSent data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Header);
        ValidatePacketHeader(data.Header);
        ValidateNonEmptyValues(data.SupportedVersions, nameof(data.SupportedVersions));
        ValidateNonNullValues(data.Frames, nameof(data.Frames));
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.PacketSentEventName, target =>
        {
            target["header"] = CreatePacketHeaderValue(data.Header);
            AddOptionalValuesArray(target, "frames", data.Frames);
            AddOptionalString(target, "stateless_reset_token", data.StatelessResetToken);
            AddOptionalStringArray(target, "supported_versions", data.SupportedVersions);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddOptionalNumber(target, "datagram_id", data.DatagramId);
            AddOptionalBoolean(target, "is_mtu_probe_packet", data.IsMtuProbePacket);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:packet_received</c> event.
    /// </summary>
    public static QlogEvent CreatePacketReceived(double time, QuicPacketReceived data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Header);
        ValidatePacketHeader(data.Header);
        ValidateNonEmptyValues(data.SupportedVersions, nameof(data.SupportedVersions));
        ValidateNonNullValues(data.Frames, nameof(data.Frames));
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.PacketReceivedEventName, target =>
        {
            target["header"] = CreatePacketHeaderValue(data.Header);
            AddOptionalValuesArray(target, "frames", data.Frames);
            AddOptionalString(target, "stateless_reset_token", data.StatelessResetToken);
            AddOptionalStringArray(target, "supported_versions", data.SupportedVersions);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddOptionalNumber(target, "datagram_id", data.DatagramId);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:packet_dropped</c> event.
    /// </summary>
    public static QlogEvent CreatePacketDropped(double time, QuicPacketDropped data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalPacketHeader(data.Header);
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.PacketDroppedEventName, target =>
        {
            AddOptionalPacketHeader(target, "header", data.Header);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddOptionalNumber(target, "datagram_id", data.DatagramId);
            AddOptionalObject(target, "details", data.Details);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:packet_buffered</c> event.
    /// </summary>
    public static QlogEvent CreatePacketBuffered(double time, QuicPacketBuffered data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalPacketHeader(data.Header);
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.PacketBufferedEventName, target =>
        {
            AddOptionalPacketHeader(target, "header", data.Header);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddOptionalNumber(target, "datagram_id", data.DatagramId);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:packets_acked</c> event.
    /// </summary>
    public static QlogEvent CreatePacketsAcked(double time, QuicPacketsAcked data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalKnownValue(data.PacketNumberSpace, nameof(data.PacketNumberSpace), PacketNumberSpaces);

        return CreateEvent(time, QlogQuicKnownValues.PacketsAckedEventName, target =>
        {
            AddOptionalString(target, "packet_number_space", data.PacketNumberSpace);
            AddOptionalNumberArray(target, "packet_numbers", data.PacketNumbers);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:udp_datagrams_sent</c> event.
    /// </summary>
    public static QlogEvent CreateUdpDatagramsSent(double time, QuicUdpDatagramsSent data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRawInfos(data.Raw, nameof(data.Raw));
        ValidateNonEmptyValues(data.Ecn, nameof(data.Ecn));

        return CreateEvent(time, QlogQuicKnownValues.UdpDatagramsSentEventName, target =>
        {
            AddOptionalNumber(target, "count", data.Count);
            AddOptionalRawInfoArray(target, "raw", data.Raw);
            AddOptionalStringArray(target, "ecn", data.Ecn);
            AddOptionalUnsignedIntArray(target, "datagram_ids", data.DatagramIds);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:udp_datagrams_received</c> event.
    /// </summary>
    public static QlogEvent CreateUdpDatagramsReceived(double time, QuicUdpDatagramsReceived data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateRawInfos(data.Raw, nameof(data.Raw));
        ValidateNonEmptyValues(data.Ecn, nameof(data.Ecn));

        return CreateEvent(time, QlogQuicKnownValues.UdpDatagramsReceivedEventName, target =>
        {
            AddOptionalNumber(target, "count", data.Count);
            AddOptionalRawInfoArray(target, "raw", data.Raw);
            AddOptionalStringArray(target, "ecn", data.Ecn);
            AddOptionalUnsignedIntArray(target, "datagram_ids", data.DatagramIds);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:udp_datagram_dropped</c> event.
    /// </summary>
    public static QlogEvent CreateUdpDatagramDropped(double time, QuicUdpDatagramDropped data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.UdpDatagramDroppedEventName, target =>
        {
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:stream_state_updated</c> event.
    /// </summary>
    public static QlogEvent CreateStreamStateUpdated(double time, QuicStreamStateUpdated data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalKnownValue(data.StreamType, nameof(data.StreamType), StreamTypes);
        ValidateOptionalKnownValue(data.Old, nameof(data.Old), StreamStates);
        ValidateKnownValue(data.New, nameof(data.New), StreamStates);
        ValidateKnownValue(data.StreamSide, nameof(data.StreamSide), StreamSides);
        ValidateInitiator(data.Trigger);

        return CreateEvent(time, QlogQuicKnownValues.StreamStateUpdatedEventName, target =>
        {
            target["stream_id"] = QlogValue.FromNumber(data.StreamId);
            AddOptionalString(target, "stream_type", data.StreamType);
            AddOptionalString(target, "old", data.Old);
            target["new"] = QlogValue.FromString(data.New);
            target["stream_side"] = QlogValue.FromString(data.StreamSide);
            AddOptionalString(target, "trigger", data.Trigger);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:stream_data_moved</c> event.
    /// </summary>
    public static QlogEvent CreateStreamDataMoved(double time, QuicStreamDataMoved data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalKnownValue(data.From, nameof(data.From), DataLocations);
        ValidateOptionalKnownValue(data.To, nameof(data.To), DataLocations);
        ValidateOptionalKnownValue(data.AdditionalInfo, nameof(data.AdditionalInfo), DataMovedAdditionalInfo);
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.StreamDataMovedEventName, target =>
        {
            AddOptionalNumber(target, "stream_id", data.StreamId);
            AddOptionalNumber(target, "offset", data.Offset);
            AddOptionalString(target, "from", data.From);
            AddOptionalString(target, "to", data.To);
            AddOptionalString(target, "additional_info", data.AdditionalInfo);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddExtensions(target, data.ExtensionData);
        });
    }

    /// <summary>
    /// Creates a <c>quic:datagram_data_moved</c> event.
    /// </summary>
    public static QlogEvent CreateDatagramDataMoved(double time, QuicDatagramDataMoved data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateOptionalKnownValue(data.From, nameof(data.From), DataLocations);
        ValidateOptionalKnownValue(data.To, nameof(data.To), DataLocations);
        ValidateOptionalRawInfo(data.Raw);

        return CreateEvent(time, QlogQuicKnownValues.DatagramDataMovedEventName, target =>
        {
            AddOptionalString(target, "from", data.From);
            AddOptionalString(target, "to", data.To);
            AddOptionalRawInfo(target, "raw", data.Raw);
            AddExtensions(target, data.ExtensionData);
        });
    }

    private static void AddOptionalNumberArray(IDictionary<string, QlogValue> target, string propertyName, IList<ulong> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, value => QlogValue.FromNumber(value));
    }

    private static void AddOptionalUnsignedIntArray(IDictionary<string, QlogValue> target, string propertyName, IList<uint> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, value => QlogValue.FromNumber((long)value));
    }

    private static void AddOptionalValuesArray(IDictionary<string, QlogValue> target, string propertyName, IList<QlogValue> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, value => value);
    }

    private static void AddOptionalRawInfo(IDictionary<string, QlogValue> target, string propertyName, QuicRawInfo? value)
    {
        if (value is not null)
        {
            target[propertyName] = CreateRawInfoValue(value);
        }
    }

    private static void AddOptionalRawInfoArray(IDictionary<string, QlogValue> target, string propertyName, IList<QuicRawInfo> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Array(values, CreateRawInfoValue);
    }

    private static void AddOptionalObject(IDictionary<string, QlogValue> target, string propertyName, IDictionary<string, QlogValue> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        target[propertyName] = QlogValueFactory.Object(writer => WriteProperties(writer, values));
    }

    private static void AddOptionalPacketHeader(IDictionary<string, QlogValue> target, string propertyName, QuicPacketHeader? value)
    {
        if (value is not null)
        {
            target[propertyName] = CreatePacketHeaderValue(value);
        }
    }

    private static QlogValue CreateRawInfoValue(QuicRawInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateRawInfo(value);

        return QlogValueFactory.Object(writer =>
        {
            if (value.Length.HasValue)
            {
                writer.WriteNumber("length", value.Length.Value);
            }

            if (value.PayloadLength.HasValue)
            {
                writer.WriteNumber("payload_length", value.PayloadLength.Value);
            }

            if (!string.IsNullOrWhiteSpace(value.Data))
            {
                writer.WriteString("data", value.Data);
            }

            WriteExtensions(writer, value.ExtensionData);
        });
    }

    private static QlogValue CreatePacketTokenValue(QuicToken value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateToken(value);

        return QlogValueFactory.Object(writer =>
        {
            if (!string.IsNullOrWhiteSpace(value.Type))
            {
                writer.WriteString("type", value.Type);
            }

            if (value.Details.Count > 0)
            {
                writer.WritePropertyName("details");
                writer.WriteStartObject();
                WriteProperties(writer, value.Details);
                writer.WriteEndObject();
            }

            if (value.Raw is not null)
            {
                writer.WritePropertyName("raw");
                CreateRawInfoValue(value.Raw).WriteTo(writer);
            }

            WriteExtensions(writer, value.ExtensionData);
        });
    }

    private static QlogValue CreatePacketHeaderValue(QuicPacketHeader value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidatePacketHeader(value);

        return QlogValueFactory.Object(writer =>
        {
            writer.WriteString("packet_type", value.PacketType);

            if (value.PacketTypeBytes.HasValue)
            {
                writer.WriteNumber("packet_type_bytes", value.PacketTypeBytes.Value);
            }

            if (value.SpinBit.HasValue)
            {
                writer.WriteBoolean("spin_bit", value.SpinBit.Value);
            }

            if (value.KeyPhase.HasValue)
            {
                writer.WriteNumber("key_phase", value.KeyPhase.Value);
            }

            if (value.KeyPhaseBit.HasValue)
            {
                writer.WriteBoolean("key_phase_bit", value.KeyPhaseBit.Value);
            }

            if (value.PacketNumberLength.HasValue)
            {
                writer.WriteNumber("packet_number_length", value.PacketNumberLength.Value);
            }

            if (value.PacketNumber.HasValue)
            {
                writer.WriteNumber("packet_number", value.PacketNumber.Value);
            }

            if (value.Token is not null)
            {
                writer.WritePropertyName("token");
                CreatePacketTokenValue(value.Token).WriteTo(writer);
            }

            if (value.Length.HasValue)
            {
                writer.WriteNumber("length", value.Length.Value);
            }

            if (!string.IsNullOrWhiteSpace(value.Version))
            {
                writer.WriteString("version", value.Version);
            }

            if (value.Scil.HasValue)
            {
                writer.WriteNumber("scil", value.Scil.Value);
            }

            if (value.Dcil.HasValue)
            {
                writer.WriteNumber("dcil", value.Dcil.Value);
            }

            if (!string.IsNullOrWhiteSpace(value.Scid))
            {
                writer.WriteString("scid", value.Scid);
            }

            if (!string.IsNullOrWhiteSpace(value.Dcid))
            {
                writer.WriteString("dcid", value.Dcid);
            }

            WriteExtensions(writer, value.ExtensionData);
        });
    }

    private static void WriteProperties(Utf8JsonWriter writer, IDictionary<string, QlogValue> properties)
    {
        foreach (KeyValuePair<string, QlogValue> property in properties)
        {
            writer.WritePropertyName(property.Key);
            property.Value.WriteTo(writer);
        }
    }

    private static void ValidatePacketHeader(QuicPacketHeader value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateKnownValue(value.PacketType, nameof(value.PacketType), PacketTypes);

        if (value.KeyPhase.HasValue && value.KeyPhaseBit.HasValue)
        {
            throw new InvalidOperationException("'KeyPhase' and 'KeyPhaseBit' cannot both be set.");
        }

        if (value.Token is not null)
        {
            ValidateToken(value.Token);
        }
    }

    private static void ValidateOptionalPacketHeader(QuicPacketHeader? value)
    {
        if (value is not null)
        {
            ValidatePacketHeader(value);
        }
    }

    private static void ValidateToken(QuicToken value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateOptionalKnownValue(value.Type, nameof(value.Type), TokenTypes);
        ValidateOptionalRawInfo(value.Raw);
    }

    private static void ValidateRawInfo(QuicRawInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Data is not null)
        {
            ValidateRequiredString(value.Data, nameof(value.Data));
        }
    }

    private static void ValidateOptionalRawInfo(QuicRawInfo? value)
    {
        if (value is not null)
        {
            ValidateRawInfo(value);
        }
    }

    private static void ValidateRawInfos(IList<QuicRawInfo> values, string parameterName)
    {
        foreach (QuicRawInfo value in values)
        {
            if (value is null)
            {
                throw new InvalidOperationException($"'{parameterName}' cannot contain null entries.");
            }

            ValidateRawInfo(value);
        }
    }

    private static void ValidateKnownValue(string? value, string parameterName, ISet<string> allowedValues)
    {
        ValidateRequiredString(value, parameterName);
        string requiredValue = value!;

        if (!allowedValues.Contains(requiredValue))
        {
            throw new InvalidOperationException($"'{parameterName}' must be one of the known values for this qlog slice.");
        }
    }

    private static void ValidateOptionalKnownValue(string? value, string parameterName, ISet<string> allowedValues)
    {
        if (value is not null)
        {
            ValidateKnownValue(value, parameterName, allowedValues);
        }
    }

    private static void ValidateNonNullValues(IList<QlogValue> values, string parameterName)
    {
        foreach (QlogValue value in values)
        {
            if (value.Kind == QlogValueKind.Null)
            {
                throw new InvalidOperationException($"'{parameterName}' cannot contain null entries.");
            }
        }
    }
}
