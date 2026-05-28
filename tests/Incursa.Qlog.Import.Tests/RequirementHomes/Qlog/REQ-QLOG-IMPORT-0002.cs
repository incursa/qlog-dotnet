// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Formats.Cbor;
using Incursa.Qlog.Import;
using Incursa.Qlog.Serialization.Cbor;
using Xunit;

namespace Incursa.Qlog.Import.Tests.RequirementHomes.Qlog;

public sealed class REQ_QLOG_IMPORT_0002
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("CoverageType", "Positive")]
    public void ContainedCborHydratesTheRetainedModel()
    {
        QlogFile file = CreateContainedCborFile();
        byte[] payload = QlogCborSerializer.Serialize(file);

        QlogFile parsed = QlogImportSerializer.DeserializeContainedCbor(new MemoryStream(payload));

        Assert.Equal(file.Title, parsed.Title);
        Assert.Equal(file.Description, parsed.Description);
        Assert.Equal(file.ExtensionData["file_extension"], parsed.ExtensionData["file_extension"]);
        Assert.Equal(file.Traces.Count, parsed.Traces.Count);
        Assert.IsType<QlogTrace>(parsed.Traces[0]);
        Assert.IsType<QlogTraceError>(parsed.Traces[1]);
        Assert.Equal(payload, QlogCborSerializer.Serialize(parsed));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("CoverageType", "Positive")]
    public void ImportSerializerSupportsStreamAutoDetectionForContainedCbor()
    {
        QlogFile file = CreateContainedCborFile();
        byte[] payload = QlogCborSerializer.Serialize(file);

        using MemoryStream stream = new(payload);
        QlogFile parsed = QlogImportSerializer.Deserialize(stream);

        Assert.Equal(payload, QlogCborSerializer.Serialize(parsed));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("CoverageType", "Negative")]
    public void ImportSerializerRejectsUnsupportedCborValueTypes()
    {
        byte[] payload = CreateUnsupportedCborPayload();

        Assert.Throws<InvalidOperationException>(() => QlogImportSerializer.DeserializeContainedCbor(new MemoryStream(payload)));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0006")]
    [Trait("CoverageType", "Negative")]
    public void ImportSerializerRejectsMalformedCborInput()
    {
        byte[] payload = QlogCborSerializer.Serialize(CreateContainedCborFile());
        byte[] truncated = payload[..^1];

        Assert.Throws<InvalidOperationException>(() => QlogImportSerializer.DeserializeContainedCbor(new MemoryStream(truncated)));
    }

    private static QlogFile CreateContainedCborFile()
    {
        QlogFile file = new()
        {
            Title = "contained-cbor-file",
            Description = "import sample",
            SerializationFormat = QlogCborKnownValues.ContainedCborSerializationFormat,
        };
        file.ExtensionData["file_extension"] = QlogValue.FromString(QlogCborKnownValues.ContainedCborFileExtension);

        QlogTrace trace = new()
        {
            Title = "contained-cbor-trace",
            CommonFields = new QlogCommonFields
            {
                GroupId = "cbor-group",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
            VantagePoint = new QlogVantagePoint
            {
                Type = QlogKnownValues.ClientVantagePoint,
            },
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.ExtensionData["trace_extension"] = QlogValue.FromArray(
            new[]
            {
                QlogValue.FromString("alpha"),
                QlogValue.FromNumber(2),
            });

        QlogEvent qlogEvent = new()
        {
            Time = 12.5,
            Name = "example:binary_serialized",
            GroupId = "cbor-group",
        };
        qlogEvent.Data["packet_number"] = QlogValue.FromNumber(7);
        qlogEvent.Data["header"] = QlogValue.Parse("""{"packet_type":"1RTT","ack_eliciting":true}""");
        qlogEvent.ExtensionData["event_extension"] = QlogValue.FromString("kept");
        trace.Events.Add(qlogEvent);
        file.Traces.Add(trace);

        QlogTraceError traceError = new()
        {
            ErrorDescription = "trace discovery failed",
            Uri = "/srv/traces/today/latest.qlog",
            VantagePoint = new QlogVantagePoint
            {
                Type = QlogKnownValues.ServerVantagePoint,
            },
        };
        traceError.ExtensionData["trace_error_extension"] = QlogValue.FromBoolean(true);
        file.Traces.Add(traceError);

        return file;
    }

    private static byte[] CreateUnsupportedCborPayload()
    {
        CborWriter writer = new(CborConformanceMode.Strict);
        writer.WriteStartMap(4);

        writer.WriteTextString("file_schema");
        writer.WriteTextString(QlogKnownValues.ContainedFileSchemaUri.OriginalString);

        writer.WriteTextString("serialization_format");
        writer.WriteTextString(QlogCborKnownValues.ContainedCborSerializationFormat);

        writer.WriteTextString("traces");
        writer.WriteStartArray(1);
        writer.WriteStartMap(2);
        writer.WriteTextString("event_schemas");
        writer.WriteStartArray(1);
        writer.WriteTextString("urn:ietf:params:qlog:events:example");
        writer.WriteEndArray();
        writer.WriteTextString("events");
        writer.WriteStartArray(0);
        writer.WriteEndArray();
        writer.WriteEndMap();
        writer.WriteEndArray();

        writer.WriteTextString("binary_extension");
        writer.WriteByteString([0x01, 0x02, 0x03]);

        writer.WriteEndMap();
        return writer.Encode();
    }
}
