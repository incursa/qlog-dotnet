# Incursa.Qlog.Import

`Incursa.Qlog.Import` is the sibling qlog import package built on `Incursa.Qlog`.

## Install

```bash
dotnet add package Incursa.Qlog.Import
```

## What It Covers

- hydration of the repository's contained qlog JSON output back into the retained model
- hydration of the repository's sequential JSON Text Sequences output back into the retained model
- hydration of the repository's contained qlog CBOR output back into the retained model
- string, Stream, and contained CBOR import entry points
- opaque-member preservation for unknown qlog data during rehydration

## Minimal Example

```csharp
using System;
using System.IO;
using Incursa.Qlog;
using Incursa.Qlog.Import;
using Incursa.Qlog.Serialization.Cbor;
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

string containedJson = QlogJsonSerializer.Serialize(file);
QlogFile contained = QlogImportSerializer.Deserialize(containedJson);

QlogFile sequentialFile = new()
{
    FileSchema = QlogKnownValues.SequentialFileSchemaUri,
    SerializationFormat = QlogKnownValues.SequentialJsonTextSequencesSerializationFormat,
};
sequentialFile.Traces.Add(trace);

string sequentialText = QlogJsonTextSequenceSerializer.Serialize(sequentialFile);

QlogFile sequential = QlogImportSerializer.Deserialize(sequentialText);

byte[] cbor = QlogCborSerializer.Serialize(file);
QlogFile containedCbor = QlogImportSerializer.DeserializeContainedCbor(new MemoryStream(cbor));
```

## Notes

- Use `QlogImportSerializer.Deserialize(...)` when you want the package to detect whether the input is contained JSON or sequential JSON Text Sequences.
- Use the explicit contained or sequential methods when you already know the format.
- Use `DeserializeContainedCbor(Stream)` when you already have repository-produced contained CBOR payloads.
- Replay execution still belongs above this package; the importer only rehydrates retained qlog model objects.
