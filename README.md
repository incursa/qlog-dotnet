# Incursa.Qlog

`Incursa.Qlog` is the .NET repository for qlog-related models, abstractions, and tooling. It is intended to support QUIC/qlog diagnostics and related structured event work.

This repository is still an early-stage scaffold. It currently provides the shared build metadata and project layout needed for requirements-driven implementation work, but no qlog feature set yet.

## Repository Layout

- `src/Incursa.Qlog`: the packable `Incursa.Qlog` library project
- `tests/Incursa.Qlog.Tests`: the companion test project scaffold
- `specs/requirements/qlog`: the canonical qlog requirements slice
- `specs/architecture/qlog`: the qlog architecture baseline
- `specs/work-items/qlog`: the qlog implementation planning slice
- `specs/verification/qlog`: the qlog verification planning slice
- `specs/generated/qlog`: provenance and scope notes for the draft source material
- `docs/requirements-workflow.md`: the repo-local SpecTrace workflow note

## Build

```bash
dotnet restore Incursa.Qlog.slnx
dotnet build Incursa.Qlog.slnx
dotnet test Incursa.Qlog.slnx
dotnet pack src/Incursa.Qlog/Incursa.Qlog.csproj -c Release
```

## Status

- Repository naming is aligned to `Incursa.Qlog`.
- The qlog baseline is split across `SPEC-QLOG-MAIN` and `SPEC-QLOG-QUIC`.
- No qlog implementation has been added yet.
