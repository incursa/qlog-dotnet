using System.Text;
using Incursa.Qlog.Import;
using Incursa.Qlog.Serialization.Cbor;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Import.Tests;

public sealed class ConsumerSmokeExamples
{
    [Fact]
    public void ImportSerializerHydratesContainedSequentialAndCborOutput()
    {
        QlogFile containedFile = CreateContainedFile();
        string containedJson = QlogJsonSerializer.Serialize(containedFile);

        QlogFile containedParsed = QlogImportSerializer.Deserialize(containedJson);
        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedParsed));

        QlogTrace containedTrace = Assert.IsType<QlogTrace>(containedFile.Traces[0]);
        QlogFile sequentialFile = CreateSequentialFile(containedTrace);
        string sequentialText = QlogJsonTextSequenceSerializer.Serialize(sequentialFile);

        QlogFile sequentialParsed = QlogImportSerializer.Deserialize(sequentialText);
        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialParsed));

        QlogFile cborFile = CreateContainedFile();
        cborFile.SerializationFormat = QlogCborKnownValues.ContainedCborSerializationFormat;
        byte[] cborPayload = QlogCborSerializer.Serialize(cborFile);

        using MemoryStream cborStream = new(cborPayload);
        QlogFile cborParsed = QlogImportSerializer.Deserialize(cborStream);

        Assert.Equal(cborPayload, QlogCborSerializer.Serialize(cborParsed));

        using MemoryStream explicitCborStream = new(cborPayload);
        QlogFile explicitCborParsed = QlogImportSerializer.DeserializeContainedCbor(explicitCborStream);

        Assert.Equal(cborPayload, QlogCborSerializer.Serialize(explicitCborParsed));
    }

    [Fact]
    public void ImportSerializerSupportsStreamAutoDetection()
    {
        QlogFile file = CreateContainedFile();
        string containedJson = QlogJsonSerializer.Serialize(file);
        QlogTrace trace = Assert.IsType<QlogTrace>(file.Traces[0]);
        string sequentialText = QlogJsonTextSequenceSerializer.Serialize(CreateSequentialFile(trace));

        using MemoryStream containedStream = new(Encoding.UTF8.GetBytes(containedJson));
        using MemoryStream sequentialStream = new(Encoding.UTF8.GetBytes(sequentialText));

        QlogFile containedParsed = QlogImportSerializer.Deserialize(containedStream);
        QlogFile sequentialParsed = QlogImportSerializer.Deserialize(sequentialStream);

        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedParsed));
        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialParsed));
    }

    [Fact]
    public void ImportSerializerCanParseExplicitContainedAndSequentialFormats()
    {
        QlogFile file = CreateContainedFile();
        string containedJson = QlogJsonSerializer.Serialize(file);
        QlogTrace trace = Assert.IsType<QlogTrace>(file.Traces[0]);
        string sequentialText = QlogJsonTextSequenceSerializer.Serialize(CreateSequentialFile(trace));

        QlogFile containedParsed = QlogImportSerializer.DeserializeContainedJson(containedJson);
        QlogFile sequentialParsed = QlogImportSerializer.DeserializeSequentialJsonTextSequences(sequentialText);

        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedParsed));
        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialParsed));
    }

    [Fact]
    public void ImportSerializerRejectsMalformedSequentialInput()
    {
        Assert.Throws<InvalidOperationException>(() => QlogImportSerializer.DeserializeSequentialJsonTextSequences("{}"));
    }

    private static QlogFile CreateContainedFile()
    {
        QlogTrace trace = new()
        {
            Title = "Replay trace",
            Description = "Smoke-test trace for import",
            CommonFields = new QlogCommonFields
            {
                Tuple = "tuple-1",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
                ReferenceTime = new QlogReferenceTime
                {
                    ClockType = QlogKnownValues.SystemClockType,
                    Epoch = "1970-01-01T00:00:00.000Z",
                    WallClockTime = "2026-05-14T12:00:00.000Z",
                },
                GroupId = "group-1",
            },
            VantagePoint = new QlogVantagePoint
            {
                Name = "client-1",
                Type = QlogKnownValues.ClientVantagePoint,
                Flow = "downstream",
            },
        };

        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.ExtensionData["trace_ext"] = QlogValue.FromString("kept");

        trace.Events.Add(new QlogEvent
        {
            Time = 1.5,
            Name = "example:connection_started",
            Tuple = "tuple-1",
            TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            GroupId = "group-1",
            Data =
            {
                ["phase"] = QlogValue.FromString("start"),
                ["count"] = QlogValue.FromNumber(1L),
            },
            SystemInfo = new Dictionary<string, QlogValue>(StringComparer.Ordinal)
            {
                ["os"] = QlogValue.FromString("windows"),
            },
        });

        trace.ExtensionData["trace_extra"] = QlogValue.FromObject(new[]
        {
            new KeyValuePair<string, QlogValue>("kind", QlogValue.FromString("example")),
        });

        trace.Events[0].ExtensionData["event_ext"] = QlogValue.FromBoolean(true);

        QlogFile file = new()
        {
            Title = "Contained replay sample",
            Description = "Smoke-test file for import",
        };
        file.Traces.Add(trace);
        file.ExtensionData["file_ext"] = QlogValue.FromArray(new[]
        {
            QlogValue.FromString("alpha"),
            QlogValue.FromString("beta"),
        });

        return file;
    }

    private static QlogFile CreateSequentialFile(QlogTrace trace)
    {
        QlogTrace sequentialTrace = new()
        {
            Title = trace.Title,
            Description = trace.Description,
            CommonFields = trace.CommonFields,
            VantagePoint = trace.VantagePoint,
        };

        foreach (Uri eventSchema in trace.EventSchemas)
        {
            sequentialTrace.EventSchemas.Add(eventSchema);
        }

        sequentialTrace.ExtensionData["trace_ext"] = QlogValue.FromString("kept");
        sequentialTrace.ExtensionData["trace_extra"] = QlogValue.FromObject(new[]
        {
            new KeyValuePair<string, QlogValue>("kind", QlogValue.FromString("example")),
        });

        foreach (QlogEvent qlogEvent in trace.Events)
        {
            QlogEvent copiedEvent = new()
            {
                Time = qlogEvent.Time,
                Name = qlogEvent.Name,
                Tuple = qlogEvent.Tuple,
                TimeFormat = qlogEvent.TimeFormat,
                GroupId = qlogEvent.GroupId,
                SystemInfo = qlogEvent.SystemInfo is null
                    ? null
                    : new Dictionary<string, QlogValue>(qlogEvent.SystemInfo, StringComparer.Ordinal),
            };

            foreach (KeyValuePair<string, QlogValue> entry in qlogEvent.Data)
            {
                copiedEvent.Data[entry.Key] = entry.Value;
            }

            foreach (KeyValuePair<string, QlogValue> entry in qlogEvent.ExtensionData)
            {
                copiedEvent.ExtensionData[entry.Key] = entry.Value;
            }

            sequentialTrace.Events.Add(copiedEvent);
        }

        QlogFile file = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
            Title = "Sequential replay sample",
            Description = "Smoke-test sequential file for import",
        };
        file.Traces.Add(sequentialTrace);
        file.ExtensionData["file_ext"] = QlogValue.FromArray(new[]
        {
            QlogValue.FromString("alpha"),
            QlogValue.FromString("beta"),
        });

        return file;
    }
}
