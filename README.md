# Incursa.Qlog

`Incursa.Qlog` is the core qlog model and serializer package for Incursa. `Incursa.Qlog.Quic` layers the bounded QUIC vocabulary and event builders on top of that core.

Use `Incursa.Qlog` when you need:

- qlog file, trace, event, and extension-data models
- contained qlog JSON serialization
- sequential qlog JSON Text Sequences serialization

Use `Incursa.Qlog.Quic` when you also need:

- QUIC-specific qlog event builders
- draft QUIC schema registration for traces
- bounded QUIC constants and payload types

## Contained vs Sequential

- Contained JSON is the default qlog envelope shape. Use it when you want a single file with a `traces` array.
- Sequential JSON Text Sequences is the draft sequential, stream-oriented format. Use it when you want one trace header record followed by event records.
- These are the only first-class serialization families in this repository today. Other qlog formats, including non-JSON encodings, stay deferred until the requirement corpus explicitly adds them.

## Minimal Core Example

```csharp
using System;
using Incursa.Qlog;
using Incursa.Qlog.Serialization.Json;

QlogFile file = new();
QlogTrace trace = new()
{
    Title = "Example trace",
    Description = "A small contained qlog example.",
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

string sequentialJsonTextSequence = QlogJsonTextSequenceSerializer.Serialize(sequentialFile, indented: true);
```

## Minimal QUIC Example

```csharp
using Incursa.Qlog;
using Incursa.Qlog.Quic;
using Incursa.Qlog.Serialization.Json;

QlogTrace trace = new();
QlogQuicEvents.RegisterDraftSchema(trace);
trace.Events.Add(QlogQuicEvents.CreateServerListening(0, new QuicServerListening
{
    IpV4 = "203.0.113.1",
    PortV4 = 443,
}));

QlogFile file = new();
file.Traces.Add(trace);

string json = QlogJsonSerializer.Serialize(file, indented: true);
```

## Repository Layout

- `src/Incursa.Qlog`: the packable core library project
- `src/Incursa.Qlog.Quic`: the packable QUIC vocabulary and event-builder library
- `tests/Incursa.Qlog.Tests`: the core requirement-homed test project
- `tests/Incursa.Qlog.Quic.Tests`: the QUIC requirement-homed test project
- `specs/requirements/qlog`: the canonical qlog requirements slice
- `specs/architecture/qlog`: the qlog architecture baseline
- `specs/work-items/qlog`: the qlog implementation planning slice
- `specs/verification/qlog`: the qlog verification planning slice
- `specs/generated/qlog`: provenance, scope, and implementation-slice notes for the draft source material
- `docs/requirements-workflow.md`: the repo-local SpecTrace workflow note
- `scripts/Refresh-QlogDraftSources.ps1`: refresh the draft snapshots and source manifest

## Build

```bash
dotnet restore Incursa.Qlog.slnx
dotnet build Incursa.Qlog.slnx
dotnet test Incursa.Qlog.slnx
dotnet pack src/Incursa.Qlog/Incursa.Qlog.csproj -c Release
dotnet pack src/Incursa.Qlog.Quic/Incursa.Qlog.Quic.csproj -c Release
```

## Status

- The first bounded qlog slices now exist: core model plus contained JSON serialization in `Incursa.Qlog`, and the QUIC lifecycle / negotiation plus transport activity vocabulary in `Incursa.Qlog.Quic`.
- Sequential JSON Text Sequences is implemented as a separate serializer boundary in `Incursa.Qlog`.
- The package boundary stays clean: `Incursa.Qlog` contains the core qlog model and serializers, while QUIC vocabulary and event builders live in `Incursa.Qlog.Quic`.
