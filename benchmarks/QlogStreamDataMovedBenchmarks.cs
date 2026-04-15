using BenchmarkDotNet.Attributes;
using Incursa.Qlog.Quic;

namespace Incursa.Qlog.Benchmarks;

[MemoryDiagnoser]
public class QlogStreamDataMovedBenchmarks
{
    private const ulong RepresentativeStreamId = 7;
    private byte[] sourcePayload = [];
    private byte[] destinationPayload = [];
    private QuicStreamDataMoved streamDataMoved = null!;
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
    }

    [Benchmark(Baseline = true)]
    public int MovePayloadWithoutLogging()
    {
        ulong offset = AdvanceOffset();
        return QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);
    }

    [Benchmark]
    public int MovePayloadWithQlogEventCreation()
    {
        ulong offset = AdvanceOffset();
        int checksum = QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);

        streamDataMoved.Offset = offset;
        var qlogEvent = QlogQuicEvents.CreateStreamDataMoved(0, streamDataMoved);
        return checksum ^ qlogEvent.Data.Count ^ qlogEvent.Name.Length;
    }

    private ulong AdvanceOffset()
    {
        nextOffset += (ulong)sourcePayload.Length;
        return nextOffset;
    }
}
