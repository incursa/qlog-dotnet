using System.Text;
using Incursa.Qlog.Import;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Import.Tests.RequirementHomes.Qlog;

public sealed class REQ_QLOG_IMPORT_0001
{
    [Fact]
    public void ContainedJsonHydratesTheRetainedModel()
    {
        QlogFile file = CreateContainedFile();
        string json = QlogJsonSerializer.Serialize(file);

        QlogFile parsed = QlogImportSerializer.DeserializeContainedJson(json);

        Assert.Equal(file.Title, parsed.Title);
        Assert.Equal(file.Description, parsed.Description);
        Assert.Single(parsed.Traces);

        QlogTrace expectedTrace = Assert.IsType<QlogTrace>(file.Traces[0]);
        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces[0]);
        Assert.Equal(expectedTrace.Title, parsedTrace.Title);
        Assert.Equal(expectedTrace.Description, parsedTrace.Description);
        Assert.Equal(expectedTrace.CommonFields?.Tuple, parsedTrace.CommonFields?.Tuple);
        Assert.Equal(expectedTrace.CommonFields?.TimeFormat, parsedTrace.CommonFields?.TimeFormat);
        Assert.Equal(expectedTrace.VantagePoint?.Type, parsedTrace.VantagePoint?.Type);
        Assert.Equal(expectedTrace.EventSchemas, parsedTrace.EventSchemas);
        Assert.Equal(expectedTrace.Events.Count, parsedTrace.Events.Count);
        Assert.Equal(expectedTrace.ExtensionData["trace_ext"], parsedTrace.ExtensionData["trace_ext"]);
        Assert.Equal(file.ExtensionData["file_ext"], parsed.ExtensionData["file_ext"]);
        Assert.Equal(json, QlogJsonSerializer.Serialize(parsed));
    }

    [Fact]
    public void SequentialJsonTextSequencesHydrateTheRetainedModel()
    {
        QlogFile file = CreateSequentialFile();
        string text = QlogJsonTextSequenceSerializer.Serialize(file);

        QlogFile parsed = QlogImportSerializer.DeserializeSequentialJsonTextSequences(text);

        Assert.Equal(QlogKnownValues.SequentialFileSchemaUri, parsed.FileSchema);
        Assert.Equal(QlogKnownValues.SequentialJsonTextSequencesSerializationFormat, parsed.SerializationFormat);
        Assert.Equal(file.Title, parsed.Title);
        Assert.Equal(file.Description, parsed.Description);
        Assert.Single(parsed.Traces);

        QlogTrace expectedTrace = Assert.IsType<QlogTrace>(file.Traces[0]);
        QlogTrace parsedTrace = Assert.IsType<QlogTrace>(parsed.Traces[0]);
        Assert.Equal(expectedTrace.Title, parsedTrace.Title);
        Assert.Equal(expectedTrace.Description, parsedTrace.Description);
        Assert.Equal(expectedTrace.CommonFields?.Tuple, parsedTrace.CommonFields?.Tuple);
        Assert.Equal(expectedTrace.VantagePoint?.Type, parsedTrace.VantagePoint?.Type);
        Assert.Equal(expectedTrace.EventSchemas, parsedTrace.EventSchemas);
        Assert.Equal(expectedTrace.Events.Count, parsedTrace.Events.Count);
        Assert.Equal(expectedTrace.ExtensionData["trace_ext"], parsedTrace.ExtensionData["trace_ext"]);
        Assert.Equal(file.ExtensionData["file_ext"], parsed.ExtensionData["file_ext"]);
        Assert.Equal(text, QlogJsonTextSequenceSerializer.Serialize(parsed));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0004")]
    public void ImportSerializerSupportsStringAndStreamAutoDetection()
    {
        QlogFile containedFile = CreateContainedFile();
        string containedJson = QlogJsonSerializer.Serialize(containedFile);

        using MemoryStream containedStream = new(Encoding.UTF8.GetBytes(containedJson));
        QlogFile containedFromString = QlogImportSerializer.Deserialize(containedJson);
        QlogFile containedFromStream = QlogImportSerializer.Deserialize(containedStream);

        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedFromString));
        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedFromStream));

        QlogFile sequentialFile = CreateSequentialFile();
        string sequentialText = QlogJsonTextSequenceSerializer.Serialize(sequentialFile);

        using MemoryStream sequentialStream = new(Encoding.UTF8.GetBytes(sequentialText));
        QlogFile sequentialFromString = QlogImportSerializer.Deserialize(sequentialText);
        QlogFile sequentialFromStream = QlogImportSerializer.Deserialize(sequentialStream);

        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialFromString));
        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialFromStream));
    }

    [Fact]
    public void ImportSerializerRejectsMalformedSequentialInput()
    {
        Assert.Throws<InvalidOperationException>(() => QlogImportSerializer.DeserializeSequentialJsonTextSequences("{}"));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-IMPORT-S1-0001")]
    public void ImportSerializerCanRoundTripExplicitFormats()
    {
        QlogFile containedFile = CreateContainedFile();
        string containedJson = QlogJsonSerializer.Serialize(containedFile);

        QlogFile containedParsed = QlogImportSerializer.DeserializeContainedJson(containedJson);
        Assert.Equal(containedJson, QlogJsonSerializer.Serialize(containedParsed));

        QlogFile sequentialFile = CreateSequentialFile();
        string sequentialText = QlogJsonTextSequenceSerializer.Serialize(sequentialFile);

        QlogFile sequentialParsed = QlogImportSerializer.DeserializeSequentialJsonTextSequences(sequentialText);
        Assert.Equal(sequentialText, QlogJsonTextSequenceSerializer.Serialize(sequentialParsed));
    }

    private static QlogFile CreateContainedFile()
    {
        QlogTrace trace = new()
        {
            Title = "Contained trace",
            Description = "Contained import sample",
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

        trace.Events[0].ExtensionData["event_ext"] = QlogValue.FromBoolean(true);

        QlogFile file = new()
        {
            Title = "Contained replay sample",
            Description = "Contained import sample",
        };
        file.Traces.Add(trace);
        file.ExtensionData["file_ext"] = QlogValue.FromArray(new[]
        {
            QlogValue.FromString("alpha"),
            QlogValue.FromString("beta"),
        });

        return file;
    }

    private static QlogFile CreateSequentialFile()
    {
        QlogFile containedFile = CreateContainedFile();
        QlogTrace containedTrace = (QlogTrace)containedFile.Traces[0];

        QlogTrace sequentialTrace = new()
        {
            Title = containedTrace.Title,
            Description = containedTrace.Description,
            CommonFields = containedTrace.CommonFields,
            VantagePoint = containedTrace.VantagePoint,
        };

        foreach (Uri eventSchema in containedTrace.EventSchemas)
        {
            sequentialTrace.EventSchemas.Add(eventSchema);
        }

        foreach (KeyValuePair<string, QlogValue> entry in containedTrace.ExtensionData)
        {
            sequentialTrace.ExtensionData[entry.Key] = entry.Value;
        }

        foreach (QlogEvent qlogEvent in containedTrace.Events)
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
            Description = "Sequential import sample",
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
