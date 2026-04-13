using System.Text;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class ConsumerSmokeExamples
{
    [Fact]
    public void CanBuildContainedAndSequentialArtifactsUsingThePublicSurface()
    {
        QlogTrace trace = new()
        {
            Title = "example-trace",
        };

        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.Events.Add(new QlogEvent
        {
            Time = 0,
            Name = "example:connection_started",
            Data =
            {
                ["header"] = QlogValue.FromObject(
                [
                    new("packet_type", QlogValue.FromString("1RTT")),
                    new("packet_number", QlogValue.FromNumber(1L)),
                ]),
                ["frames"] = QlogValue.FromArray(
                [
                    QlogValue.FromString("ack"),
                    QlogValue.FromString("crypto"),
                ]),
            },
        });

        QlogFile containedFile = new()
        {
            Title = "contained-demo",
        };
        containedFile.Traces.Add(trace);

        string containedJson = QlogJsonSerializer.Serialize(containedFile);

        Assert.Contains("\"file_schema\":\"urn:ietf:params:qlog:file:contained\"", containedJson, StringComparison.Ordinal);
        Assert.Contains("\"header\":{\"packet_type\":\"1RTT\",\"packet_number\":1}", containedJson, StringComparison.Ordinal);
        Assert.Contains("\"frames\":[\"ack\",\"crypto\"]", containedJson, StringComparison.Ordinal);

        QlogFile sequentialFile = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
        };
        sequentialFile.Traces.Add(trace);

        string jsonTextSequence = QlogJsonTextSequenceSerializer.Serialize(sequentialFile);

        Assert.StartsWith("\u001e{\"file_schema\":\"urn:ietf:params:qlog:file:sequential\"", jsonTextSequence, StringComparison.Ordinal);
        Assert.Contains("\"name\":\"example:connection_started\"", jsonTextSequence, StringComparison.Ordinal);
    }

    [Fact]
    public void CanWriteContainedAndSequentialArtifactsToStreams()
    {
        QlogTrace trace = new()
        {
            Title = "example-trace",
        };

        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
        trace.Events.Add(new QlogEvent
        {
            Time = 0,
            Name = "example:connection_started",
        });

        QlogFile containedFile = new();
        containedFile.Traces.Add(trace);

        string containedJson;
        using (MemoryStream containedStream = new())
        {
            QlogJsonSerializer.Serialize(containedStream, containedFile);
            containedJson = Encoding.UTF8.GetString(containedStream.ToArray());
        }

        Assert.StartsWith("{\"file_schema\":\"urn:ietf:params:qlog:file:contained\"", containedJson, StringComparison.Ordinal);

        QlogFile sequentialFile = new()
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
        };
        sequentialFile.Traces.Add(trace);

        string sequentialJsonTextSequence;
        using (MemoryStream sequentialStream = new())
        {
            QlogJsonTextSequenceSerializer.Serialize(sequentialStream, sequentialFile);
            sequentialJsonTextSequence = Encoding.UTF8.GetString(sequentialStream.ToArray());
        }

        Assert.StartsWith("\u001e{\"file_schema\":\"urn:ietf:params:qlog:file:sequential\"", sequentialJsonTextSequence, StringComparison.Ordinal);
        Assert.EndsWith("\n", sequentialJsonTextSequence, StringComparison.Ordinal);
    }
}
