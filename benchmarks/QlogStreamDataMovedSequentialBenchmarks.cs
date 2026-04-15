using BenchmarkDotNet.Attributes;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;

namespace Incursa.Qlog.Benchmarks;

[MemoryDiagnoser]
public class QlogStreamDataMovedSequentialBenchmarks
{
    private const ulong RepresentativeStreamId = 7;
    private byte[] sourcePayload = [];
    private byte[] destinationPayload = [];
    private QuicStreamDataMoved streamDataMoved = null!;
    private QlogFile sequentialFile = null!;
    private QlogTrace sequentialTrace = null!;
    private ulong nextOffset;

    [GlobalSetup]
    public void GlobalSetup()
    {
        sourcePayload = QlogTransportBenchmarkData.CreatePayload(QlogTransportBenchmarkData.RepresentativePayloadLength);
        destinationPayload = new byte[sourcePayload.Length];
        streamDataMoved = new QuicStreamDataMoved
        {
            StreamId = RepresentativeStreamId,
            From = QlogQuicKnownValues.DataLocationApplication,
            To = QlogQuicKnownValues.DataLocationTransport,
            Raw = new QuicRawInfo
            {
                Length = (ulong)sourcePayload.Length,
                PayloadLength = (ulong)sourcePayload.Length,
            },
        };

        sequentialTrace = new QlogTrace
        {
            CommonFields = new QlogCommonFields
            {
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
        };

        QlogQuicEvents.RegisterDraftSchema(sequentialTrace);
        sequentialTrace.Events.Add(QlogQuicEvents.CreateStreamDataMoved(0, streamDataMoved));

        sequentialFile = new QlogFile
        {
            FileSchema = QlogKnownValues.SequentialFileSchemaUri,
            SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
        };

        sequentialFile.Traces.Add(sequentialTrace);
    }

    [Benchmark(Baseline = true)]
    public int MovePayloadWithoutLogging()
    {
        ulong offset = AdvanceOffset();
        return QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);
    }

    [Benchmark]
    public int MovePayloadWithSequentialQlog()
    {
        ulong offset = AdvanceOffset();
        int checksum = QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);

        streamDataMoved.Offset = offset;
        sequentialTrace.Events[0] = QlogQuicEvents.CreateStreamDataMoved(0, streamDataMoved);
        QlogJsonTextSequenceSerializer.Serialize(Stream.Null, sequentialFile, indented: false);

        return checksum ^ sequentialTrace.Events[0].Data.Count;
    }

    private ulong AdvanceOffset()
    {
        nextOffset += (ulong)sourcePayload.Length;
        return nextOffset;
    }
}
