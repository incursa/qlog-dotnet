# Incursa.Qlog Agent Guidance

Use `@delivery_director` to triage and delegate. If this is clearly a single-lane task, skip delegation and use the narrowest specialist.

## Scope

- Keep this repository lean and requirements-driven.
- Do not add qlog serialization, mapping, or transport logic until the requirements corpus exists.
- Keep names, namespaces, projects, and package metadata aligned to `Incursa.Qlog`.
- Keep the qlog SpecTrace corpus split across `specs/requirements/qlog`, `specs/architecture/qlog`, `specs/work-items/qlog`, `specs/verification/qlog`, and `specs/generated/qlog`.

## Repository Shape

- `src/Incursa.Qlog` is the packable library root.
- `tests/Incursa.Qlog.Tests` is the companion test project scaffold.
- `specs/requirements/qlog` is the canonical qlog requirement slice.
- `specs/architecture/qlog`, `specs/work-items/qlog`, and `specs/verification/qlog` hold the planning artifacts that trace back to those requirements.
- `specs/generated/qlog` holds provenance and scope notes for the draft sources used to author the baseline corpus.

## Guardrails

- Keep the first baseline limited to the draft qlog main-schema and quic-events documents until the repository needs more scope.
- Prefer the local `.NET` and SpecTrace conventions already used in the Incursa repositories when adding new files.
