# Incursa.Qlog.Quic

`Incursa.Qlog.Quic` is the bounded QUIC vocabulary package that builds on `Incursa.Qlog`.

## Install

```bash
dotnet add package Incursa.Qlog.Quic
```

## What It Covers

- draft QUIC schema registration for qlog traces
- bounded QUIC event builders for lifecycle, negotiation, packet activity, stream movement, and recovery
- bounded payload types for the QUIC slices already implemented in this repository

## Minimal Example

```csharp
using Incursa.Qlog;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;

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

QlogFile file = new();
file.Traces.Add(trace);

string json = QlogJsonSerializer.Serialize(file, indented: true);
```

## Notes

- The package depends on `Incursa.Qlog` for the core qlog model, contained JSON serializer, and generic value infrastructure.
- The package surface is intentionally bounded to the implemented QUIC event families in this repository.
- Repository details and scope notes live at `https://github.com/incursa/qlog-dotnet`.
