# Qlog Scope Boundary

This note records the first baseline boundary for `Incursa.Qlog`.

## In Scope

- qlog file envelopes and trace/container structure
- trace metadata, common fields, timestamps, event identity, and schema URIs
- serialization-independent core qlog model support
- JSON and JSON Text Sequences compatibility where the draft main-schema supports it
- QUIC event vocabulary and qlog-to-QUIC mapping support

## Out Of Scope For The First Baseline

- HTTP/3 event mapping
- generated coverage triage and chunk-manifest machinery
- large work-item backlogs
- non-JSON sinks and transport layers that are not required to represent the draft qlog model
- any behavior not covered by the two draft source documents listed in the provenance note

## Why The Boundary Exists

- The source material is draft-state and still moving.
- The repository needs a stable requirements corpus before implementation work begins.
- Keeping the boundary small makes later implementation prompts easier to anchor to one requirement slice at a time.
