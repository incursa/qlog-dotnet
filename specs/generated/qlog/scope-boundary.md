# Qlog V1 Scope Boundary

This note records the v1 implementation boundary for `Incursa.Qlog`.
It is intentionally narrower than the full draft surface so the repository can
start implementation work without absorbing a QUIC-sized backlog.

## In Scope For V1

- qlog file envelopes and trace/container structure
- trace metadata, common fields, timestamps, event identity, and schema URIs
- serialization-neutral core qlog model contracts
- first-class contained JSON and sequential JSON Text Sequences writer boundaries over the shared model
- a small writer/sink boundary that can support contained JSON and qlog-compatible output
- the sibling `Incursa.Qlog.Quic` package for the recorded QUIC event vocabulary and mapping layer
- QUIC event vocabulary registration and mapping for the recorded draft revision
- extension handling that preserves unknown fields and draft-version drift

## Out Of Scope For V1

- HTTP/3 event mapping
- advanced viewer, CLI, or UI tooling
- broad runtime diagnostics integration or machine-wide tracing control
- non-JSON sinks such as CSV, protobuf, or flatbuffers, and any CBOR publication path beyond the implemented contained serializer sibling package
- parser dependencies or replay-oriented hydration logic inside `Incursa.Qlog` core
- additional protocol vocabularies beyond the recorded QUIC draft
- large backlog automation, chunk manifests, or proof-generation machinery
- any behavior not covered by the two draft source documents listed in the provenance note

## Why The Boundary Exists

- The source material is draft-state and still moving.
- The repository needs a stable implementation plan before code starts.
- Keeping the first pass small enough to reason about makes later requirement-by-requirement work tractable.
- The v1 implementation boundary only justified JSON and JSON Text Sequences at the outset. The contained CBOR follow-on now exists as a sibling package, and the sibling import package now covers contained CBOR hydration as well, so a general multi-format abstraction would still add complexity beyond the implemented slices.
- Import and replay-oriented parsing should follow the same sibling-package pattern so the hot logging package does not pick up parser dependencies or replay state machines.

## Slice Order

The implementation plan is summarized in
[`implementation-slices.md`](implementation-slices.md).
The intended order is core model and envelope first, then QUIC vocabulary
foundation, then transport activity, then migration/recovery state, then the
sequential `JSON Text Sequences` slice, then contained CBOR serialization, and
then the import and rehydration slice.
