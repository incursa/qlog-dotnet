# Maintainer Readiness

This document is the local maintainer handoff for `Incursa.Qlog`. It records what the repository is for, what is stable enough to rely on, how to validate it locally, and what remains open.

## Repository Purpose

`Incursa.Qlog` is a requirements-driven .NET qlog library set. The repository owns the retained qlog model, JSON and CBOR serialization surfaces, import and rehydration helpers, a bounded QUIC vocabulary package, and the SpecTrace corpus that defines the current implementation boundary.

The core package is not QUIC-specific. QUIC support is intentionally isolated in `Incursa.Qlog.Quic` so the qlog model and serializers can also support non-QUIC operation timelines when those behaviors are represented in the requirements corpus.

## Public Package Surface

The repository builds four packable packages:

- `Incursa.Qlog`: core qlog file, trace, event, value, vantage-point, reference-time, extension-data, contained JSON, and sequential JSON Text Sequences surface.
- `Incursa.Qlog.Cbor`: sibling contained CBOR serializer built on the retained `Incursa.Qlog` model.
- `Incursa.Qlog.Import`: sibling import and rehydration utilities for contained JSON, sequential JSON Text Sequences, and contained CBOR artifacts.
- `Incursa.Qlog.Quic`: bounded QUIC event vocabulary, known values, payload types, schema metadata, and event builders built on `Incursa.Qlog`.

Public API compatibility is tracked with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files in each package project. Treat shipped baseline changes as release-significant.

## Architecture And Layout

- `Incursa.Qlog.slnx` includes the package projects, benchmark project, and test projects.
- `src/Incursa.Qlog/` contains the core retained model plus JSON serializers and capture/sink infrastructure.
- `src/Incursa.Qlog.Cbor/` contains contained CBOR serialization over the retained model.
- `src/Incursa.Qlog.Import/` contains import and rehydration entry points without moving parser dependencies into the core package.
- `src/Incursa.Qlog.Quic/` contains the QUIC vocabulary layer and typed event builders.
- `tests/Incursa.Qlog.Tests/` covers the core model, JSON serializers, capture, sinks, CBOR writer behavior, and release-versioning scripts.
- `tests/Incursa.Qlog.Import.Tests/` covers import and rehydration behavior.
- `tests/Incursa.Qlog.Quic.Tests/` covers QUIC vocabulary behavior and recorded fixture hydration.
- `benchmarks/` contains BenchmarkDotNet baselines for representative qlog overhead checks.
- `specs/requirements/qlog/` is the canonical requirements surface.
- `specs/architecture/qlog/`, `specs/work-items/qlog/`, and `specs/verification/qlog/` trace design, planned work, and proof expectations back to the requirements.
- `specs/generated/qlog/` records source provenance, source snapshots, implementation-slice notes, and scope-boundary material.

The current design keeps serializer/import families as sibling packages when they would add dependencies or evolution pressure to the core model. Do not fold import, replay, or protocol-specific vocabulary into `Incursa.Qlog` unless the requirements and architecture are updated first.

## Local Build And Test

Use local commands as the proof surface. Do not treat GitHub Actions as the proof for maintainer changes.

```powershell
dotnet restore Incursa.Qlog.slnx
dotnet build Incursa.Qlog.slnx --no-restore --configuration Release
dotnet test Incursa.Qlog.slnx --no-build --configuration Release -v minimal
```

Run SpecTrace validation when requirements, architecture, work items, verification artifacts, or SpecTrace helper scripts change:

```powershell
pwsh -NoProfile -File scripts/Validate-SpecTraceJson.ps1
pwsh -NoProfile -File scripts/Test-RequirementHomeCoverage.ps1
```

Run the public API/versioning smoke before release-facing changes:

```powershell
pwsh -NoProfile -File scripts/release/Invoke-ReleaseVersioning.ps1 -CalculateOnly
pwsh -NoProfile -File scripts/release/validate-public-api-versioning.ps1 -Tag v1.1.4
```

Use the target tag that matches the release being prepared. The `v1.1.4` example is only a local policy check target after `v1.1.3`.

## Pack Validation

Build first, then pack all package projects with `--no-build` so packaging is validated against the same compiled output:

```powershell
New-Item -ItemType Directory -Force artifacts/packages | Out-Null
dotnet pack src/Incursa.Qlog/Incursa.Qlog.csproj --configuration Release --no-build --output artifacts/packages /p:ContinuousIntegrationBuild=true
dotnet pack src/Incursa.Qlog.Cbor/Incursa.Qlog.Cbor.csproj --configuration Release --no-build --output artifacts/packages /p:ContinuousIntegrationBuild=true
dotnet pack src/Incursa.Qlog.Import/Incursa.Qlog.Import.csproj --configuration Release --no-build --output artifacts/packages /p:ContinuousIntegrationBuild=true
dotnet pack src/Incursa.Qlog.Quic/Incursa.Qlog.Quic.csproj --configuration Release --no-build --output artifacts/packages /p:ContinuousIntegrationBuild=true
```

Do not commit `artifacts/`, `bin/`, or `obj/` outputs.

## Release And Versioning

`Directory.Build.props` owns the package version and shared NuGet metadata. The current package version is `1.1.3`.

Release readiness depends on:

- a clean working tree before release preparation;
- current shipped/unshipped public API baselines for every package;
- passing solution restore, build, tests, SpecTrace validation, and package validation;
- a release tag of the form `v<major>.<minor>.<patch>`;
- `NUGET_API_KEY` being available only to the publish workflow or trusted local release environment.

The release helper can calculate the next version from the latest release tag and public API baseline deltas:

```powershell
pwsh -NoProfile -File scripts/release/Invoke-ReleaseVersioning.ps1 -CalculateOnly
```

Do not publish or push release tags as part of routine documentation readiness work.

## Current Readiness Status

Local maintainer readiness is strong when the local restore, build, test, SpecTrace validation, requirement-home coverage audit, package validation, and `git diff --check` all pass.

Stable enough to rely on:

- core qlog retained model and contained JSON/sequential JSON Text Sequences serializers;
- contained CBOR serializer in the sibling package;
- import and rehydration for contained JSON, sequential JSON Text Sequences, and contained CBOR;
- bounded QUIC vocabulary and typed event builders for the implemented event families;
- requirement-homed tests and SpecTrace validation over the qlog corpus;
- public API baseline tracking for package compatibility review.

Still under active maintenance:

- qlog draft parity, because the recorded source documents remain draft-state inputs;
- source snapshot refresh, because the local manifest intentionally points at `draft-ietf-quic-qlog-main-schema-13`;
- future protocol vocabularies, including HTTP/3 events, which remain outside the current baseline;
- governance surfaces such as contribution, security, CLA, issue-template, and PR-template files, which are not part of this readiness slice.

## Known Gaps And Future Work

The authoritative open requirement gaps live in [`../specs/requirements/qlog/REQUIREMENT-GAPS.md`](../specs/requirements/qlog/REQUIREMENT-GAPS.md). Current standing gaps are:

- `draft-volatility`: revisit requirements and fixtures when the qlog main-schema or QUIC event drafts advance.
- `source-provenance-refresh`: refresh the local source manifest and text snapshots when updating the recorded draft baseline.
- `future-h3-events`: keep HTTP/3 event mapping out of the current qlog baseline until it has its own requirement family.

Repo-maintenance gaps from the local Incursa audit:

- `CONTRIBUTING.md`, `SECURITY.md`, `CODE_OF_CONDUCT.md`, CLA surfaces, PR template, and issue templates are absent.
- `docs/contributor-agreement-automation.md`, `SUPPORT.md`, `CODEOWNERS`, and Dependabot are also absent.

Treat those governance items as owner/maintainer follow-up unless the task explicitly asks for repository governance normalization.
