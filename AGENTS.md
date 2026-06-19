# Incursa.Qlog Agent Guidance

Use `@delivery_director` to triage and delegate. If this is clearly a single-lane task, skip delegation and use the narrowest specialist.

## Scope

- Keep this repository lean and requirements-driven.
- Do not add qlog serialization, mapping, or transport logic until the requirements corpus exists.
- Keep names, namespaces, projects, and package metadata aligned to `Incursa.Qlog`.
- Keep the qlog SpecTrace corpus split across `specs/requirements/qlog`, `specs/architecture/qlog`, `specs/work-items/qlog`, `specs/verification/qlog`, and `specs/generated/qlog`.

## Repository Shape

- `src/Incursa.Qlog` is the packable core qlog model and JSON serializer package.
- `src/Incursa.Qlog.Cbor` is the packable sibling contained CBOR serializer package.
- `src/Incursa.Qlog.Import` is the packable sibling import and rehydration package.
- `src/Incursa.Qlog.Quic` is the packable bounded QUIC vocabulary package.
- `tests/Incursa.Qlog.Tests` covers core, capture, sink, CBOR writer, and release-versioning behavior.
- `tests/Incursa.Qlog.Import.Tests` covers import and rehydration behavior.
- `tests/Incursa.Qlog.Quic.Tests` covers bounded QUIC vocabulary behavior and fixture hydration.
- `specs/requirements/qlog` is the canonical qlog requirement slice.
- `specs/architecture/qlog`, `specs/work-items/qlog`, and `specs/verification/qlog` hold the planning artifacts that trace back to those requirements.
- `specs/generated/qlog` holds provenance and scope notes for the draft sources used to author the baseline corpus.

## Local Validation

Use local validation as the proof surface:

```powershell
dotnet restore Incursa.Qlog.slnx
dotnet build Incursa.Qlog.slnx --no-restore --configuration Release
dotnet test Incursa.Qlog.slnx --no-build --configuration Release -v minimal
pwsh -NoProfile -File scripts/Validate-SpecTraceJson.ps1
pwsh -NoProfile -File scripts/Test-RequirementHomeCoverage.ps1
git diff --check
```

For release or package-surface changes, also pack all four package projects, including `Incursa.Qlog.Import`.

## Guardrails

- Keep the first baseline limited to the draft qlog main-schema and quic-events documents until the repository needs more scope.
- Prefer the local `.NET` and SpecTrace conventions already used in the Incursa repositories when adding new files.
