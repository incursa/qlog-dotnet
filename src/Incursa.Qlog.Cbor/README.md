# Incursa.Qlog.Cbor

`Incursa.Qlog.Cbor` is the sibling CBOR serialization package built on `Incursa.Qlog`.

## Install

```bash
dotnet add package Incursa.Qlog.Cbor
```

## What It Covers

- contained qlog CBOR artifacts over the retained `Incursa.Qlog` model
- stream-first binary serialization
- draft-compatible contained file metadata using the standard contained qlog file schema URI

## Minimal Example

```csharp
using System;
using System.IO;
using Incursa.Qlog;
using Incursa.Qlog.Serialization.Cbor;

QlogFile file = new()
{
    SerializationFormat = QlogCborKnownValues.ContainedCborSerializationFormat,
};

QlogTrace trace = new()
{
    Title = "Example trace",
    VantagePoint = new QlogVantagePoint
    {
        Type = QlogKnownValues.ClientVantagePoint,
    },
};

trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));
trace.Events.Add(new QlogEvent
{
    Time = 0,
    Name = "example:connection_started",
});

file.Traces.Add(trace);

byte[] cbor = QlogCborSerializer.Serialize(file);

using FileStream stream = File.Create("example.qlog.cbor");
QlogCborSerializer.Serialize(stream, file);
```

## Notes

- The sibling package depends on `Incursa.Qlog` for the retained qlog model and core validation rules.
- The first CBOR slice is contained-file only. It does not define a sequential or append-oriented CBOR framing format.
- Contained CBOR artifacts reuse the contained qlog file schema URI, advertise `application/cbor`, and use `.qlog.cbor` as the canonical file extension in this repository.
- Repository details and scope notes live at `https://github.com/incursa/qlog-dotnet`.
