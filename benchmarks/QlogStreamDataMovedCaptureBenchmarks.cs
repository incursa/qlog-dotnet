using BenchmarkDotNet.Attributes;
using Incursa.Qlog.Quic;

namespace Incursa.Qlog.Benchmarks;

[MemoryDiagnoser]
public class QlogStreamDataMovedCaptureBenchmarks
{
    private const ulong RepresentativeStreamId = 7;
    private byte[] sourcePayload = [];
    private byte[] destinationPayload = [];
    private QuicStreamDataMoved streamDataMoved = null!;
    private QlogTrace traceHeader = null!;
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

        traceHeader = new QlogTrace
        {
            CommonFields = new QlogCommonFields
            {
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
        };

        QlogQuicEvents.RegisterDraftSchema(traceHeader);
    }

    [Benchmark(Baseline = true)]
    public int MovePayloadWithoutLogging()
    {
        ulong offset = AdvanceOffset();
        return QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);
    }

    [Benchmark]
    public int MovePayloadWithCaptureAndStreamSink()
    {
        ulong offset = AdvanceOffset();
        int checksum = QlogTransportBenchmarkData.MovePayload(sourcePayload, destinationPayload, offset);

        streamDataMoved.Offset = offset;
        QlogEvent qlogEvent = QlogQuicEvents.CreateStreamDataMoved(0, streamDataMoved);
        QlogCaptureSession session = new("benchmark-session", traceHeader, fileTitle: "benchmark-file");
        using MemoryStream stream = new();
        QlogCaptureCoordinator coordinator = new();
        QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, new QlogStreamCaptureSink(stream));

        try
        {
            coordinator.Capture(session, qlogEvent);
            coordinator.CompleteSessionAsync(session).GetAwaiter().GetResult();
            return checksum ^ (int)stream.Length;
        }
        finally
        {
            subscription.DisposeAsync().GetAwaiter().GetResult();
            coordinator.DisposeAsync().GetAwaiter().GetResult();
        }
    }

    private ulong AdvanceOffset()
    {
        nextOffset += (ulong)sourcePayload.Length;
        return nextOffset;
    }
}
