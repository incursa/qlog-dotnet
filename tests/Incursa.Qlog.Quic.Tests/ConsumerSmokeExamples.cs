using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Quic.Tests;

public sealed class ConsumerSmokeExamples
{
    [Fact]
    public void CanBuildAQuicTraceUsingTheBoundedEventBuilders()
    {
        QlogTrace trace = new()
        {
            Title = "client-handshake",
        };

        QlogQuicEvents.RegisterDraftSchema(trace);
        trace.Events.Add(QlogQuicEvents.CreateConnectionStarted(
            0,
            new QuicConnectionStarted
            {
                Local = new QuicTupleEndpointInfo
                {
                    IpV4 = "203.0.113.10",
                    PortV4 = 443,
                },
                Remote = new QuicTupleEndpointInfo
                {
                    IpV4 = "198.51.100.20",
                    PortV4 = 443,
                },
            }));
        trace.Events.Add(QlogQuicEvents.CreateConnectionStateUpdated(
            1,
            new QuicConnectionStateUpdated
            {
                New = QlogQuicKnownValues.ConnectionStateHandshakeStarted,
            }));

        QlogFile file = new()
        {
            Title = "quic-demo",
        };
        file.Traces.Add(trace);

        string json = QlogJsonSerializer.Serialize(file);

        Assert.Contains($"\"name\":\"{QlogQuicKnownValues.ConnectionStartedEventName}\"", json, StringComparison.Ordinal);
        Assert.Contains(QlogQuicKnownValues.ConnectionStateHandshakeStarted, json, StringComparison.Ordinal);
        Assert.Contains("203.0.113.10", json, StringComparison.Ordinal);
    }
}
