# Incursa.Qlog

`Incursa.Qlog` is the .NET repository for qlog-related models, abstractions, and tooling. It is intended to support qlog diagnostics and related structured event work.

This repository is still an early-stage scaffold, but the first bounded qlog slices are now implemented and split across two packages: the contained JSON core model in `Incursa.Qlog`, and the QUIC lifecycle / negotiation plus transport activity vocabulary in `Incursa.Qlog.Quic`.

## Repository Layout

- `src/Incursa.Qlog`: the packable `Incursa.Qlog` core library project
- `src/Incursa.Qlog.Quic`: the packable `Incursa.Qlog.Quic` vocabulary and mapping library
- `tests/Incursa.Qlog.Tests`: the core requirement-homed test project
- `tests/Incursa.Qlog.Quic.Tests`: the QUIC requirement-homed test project
- `specs/requirements/qlog`: the canonical qlog requirements slice
- `specs/architecture/qlog`: the qlog architecture baseline
- `specs/work-items/qlog`: the qlog implementation planning slice
- `specs/verification/qlog`: the qlog verification planning slice
- `specs/generated/qlog`: provenance, scope, and implementation-slice notes for the draft source material
- `specs/generated/qlog/source-docs`: local text snapshots for the recorded draft revisions
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

- Repository naming is aligned to `Incursa.Qlog`.
- The qlog v1 planning baseline is split across the qlog requirements, architecture, work-item, verification, and implementation-slice artifacts.
- The first bounded v1 slices now exist: core model plus contained JSON serialization in `Incursa.Qlog`, and the QUIC lifecycle / negotiation plus transport activity vocabulary in `Incursa.Qlog.Quic`.
- Sequential JSON Text Sequences now exists as a separate serializer boundary in `Incursa.Qlog`.
