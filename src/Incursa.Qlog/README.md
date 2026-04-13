# Incursa.Qlog

`Incursa.Qlog` is the packable core qlog library. It provides the model types, value infrastructure, and JSON serializers that `Incursa.Qlog.Quic` builds on.

## Install

```bash
dotnet add package Incursa.Qlog
```

## What It Covers

- contained qlog JSON envelopes
- sequential qlog JSON Text Sequences envelopes
- qlog file, trace, event, reference-time, and vantage-point model types
- round-trippable extension data for unknown members

## When To Use Which Serializer

- Use `QlogJsonSerializer` for contained qlog files with a `traces` array.
- Use `QlogJsonTextSequenceSerializer` for the draft sequential format when you have one trace header record followed by event records.

## Minimal Example

```csharp
using System;
using Incursa.Qlog;
using Incursa.Qlog.Serialization.Json;

QlogFile file = new();
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

string containedJson = QlogJsonSerializer.Serialize(file, indented: true);

QlogFile sequentialFile = new()
{
    FileSchema = QlogKnownValues.SequentialFileSchemaUri,
    SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
};
sequentialFile.Traces.Add(trace);

string jsonTextSequence = QlogJsonTextSequenceSerializer.Serialize(sequentialFile, indented: true);
```

## Notes

- Unknown file, trace, common-field, vantage-point, reference-time, and event members are preserved through explicit extension data.
- The core package stays free of QUIC-specific vocabulary; that surface lives in `Incursa.Qlog.Quic`.
- The package is intentionally JSON-first today: contained JSON and sequential JSON Text Sequences are the only first-class formats.
- Other serialization families, including non-JSON encodings, remain deferred until a later requirement slice justifies a separate format-specific boundary.
- Repository details and scope notes live at `https://github.com/incursa/qlog-dotnet`.
