# Incursa.Qlog

[![CI](https://github.com/incursa/qlog-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/incursa/qlog-dotnet/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/incursa/qlog-dotnet)](LICENSE)

`Incursa.Qlog` is a .NET qlog library set with a requirements-driven repository. It includes the packable core qlog package, the bounded QUIC companion package, and the SpecTrace corpus that defines the repository scope.

## Packages

- [`Incursa.Qlog`](src/Incursa.Qlog/README.md): core qlog models, common fields, value infrastructure, and JSON serializers for contained and sequential artifacts.
- [`Incursa.Qlog.Quic`](src/Incursa.Qlog.Quic/README.md): bounded QUIC vocabulary, payloads, schema registration, and event builders built on `Incursa.Qlog`.

## Scope

- qlog file, trace, event, vantage-point, reference-time, and extension-data models
- contained JSON serialization and sequential JSON Text Sequences serialization
- QUIC draft event vocabulary and helper types that map to the scoped requirements corpus
- requirements, architecture, work items, verification, and provenance notes under `specs/`
- excludes broader protocol vocabularies, transports, and additional qlog serialization families until the requirements corpus adds them

## Install

Install the core package when you need the qlog model and serializers:

```bash
dotnet add package Incursa.Qlog
```

Add the QUIC package when you also need the scoped QUIC vocabulary:

```bash
dotnet add package Incursa.Qlog.Quic
```

## Build

```bash
dotnet restore Incursa.Qlog.slnx
dotnet build Incursa.Qlog.slnx -c Release
dotnet test Incursa.Qlog.slnx -c Release
dotnet pack src/Incursa.Qlog/Incursa.Qlog.csproj -c Release
dotnet pack src/Incursa.Qlog.Quic/Incursa.Qlog.Quic.csproj -c Release
```

## Start Here

- Core package guide: [`src/Incursa.Qlog/README.md`](src/Incursa.Qlog/README.md)
- QUIC package guide: [`src/Incursa.Qlog.Quic/README.md`](src/Incursa.Qlog.Quic/README.md)
- Requirements workflow: [`docs/requirements-workflow.md`](docs/requirements-workflow.md)
- SpecTrace prep note: [`docs/spec-trace-prep.md`](docs/spec-trace-prep.md)

## Repository Layout

- `src/Incursa.Qlog`: packable core library project
- `src/Incursa.Qlog.Quic`: packable QUIC vocabulary and event-builder project
- `tests/Incursa.Qlog.Tests`: requirement-homed tests for the core package
- `tests/Incursa.Qlog.Quic.Tests`: requirement-homed tests for the QUIC package
- `specs/requirements/qlog`: canonical qlog requirements slice
- `specs/architecture/qlog`: qlog architecture artifacts
- `specs/work-items/qlog`: implementation planning artifacts
- `specs/verification/qlog`: verification artifacts
- `specs/generated/qlog`: provenance, boundary, and derived planning support material

## Tooling

- `.pre-commit-config.yaml` wires up formatting, JSON/YAML/XML validation, SpecTrace validation, and release smoke checks.
- `.githooks/pre-commit` and `.githooks/pre-push` invoke `pre-commit` through the repo-local hook path.
- [`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs restore, build, validation, tests, and packing on branch pushes and pull requests.
- [`.github/workflows/publish-nuget-packages.yml`](.github/workflows/publish-nuget-packages.yml) validates public API release policy before packing and publishing tagged releases.
- [`scripts/setup-git-hooks.ps1`](scripts/setup-git-hooks.ps1) configures Git to use the repo-local hook path.
- [`scripts/Validate-SpecTraceJson.ps1`](scripts/Validate-SpecTraceJson.ps1) validates the canonical qlog SpecTrace corpus.

To install the local hook path after cloning:

```bash
pwsh scripts/setup-git-hooks.ps1
python -m pip install pre-commit
```

## Contributing

- Keep names, namespaces, projects, and package metadata aligned to `Incursa.Qlog`.
- Drive behavior changes from the owning artifacts under `specs/requirements/qlog` before expanding implementation scope.
- Prefer durable repository documentation over time-sensitive progress notes in committed Markdown.

## License

MIT. See [`LICENSE`](LICENSE).
