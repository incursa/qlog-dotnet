# Incursa.Qlog Agent Guidance

Use `@delivery_director` to triage and delegate. If this is clearly a single-lane task, skip delegation and use the narrowest specialist.

## Scope

- Keep this repository lean and requirements-driven.
- Do not add qlog serialization, mapping, or transport logic until the requirements corpus exists.
- Keep names, namespaces, projects, and package metadata aligned to `Incursa.Qlog`.

## Repository Shape

- `src/Incursa.Qlog` is the packable library root.
- `tests/Incursa.Qlog.Tests` is the companion test project scaffold.
- `specs/requirements` and `specs/verification` are the future home for canonical requirements and verification artifacts.

## Guardrails

- Do not introduce SpecTrace work items or other trace artifacts unless that corpus is being created intentionally.
- Prefer the local `.NET` repository conventions already used in the Incursa repositories when adding new files.

