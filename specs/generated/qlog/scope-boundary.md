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
- additional protocol vocabularies beyond the recorded QUIC draft
- large backlog automation, chunk manifests, or proof-generation machinery
- any behavior not covered by the two draft source documents listed in the provenance note

## Why The Boundary Exists

- The source material is draft-state and still moving.
- The repository needs a stable implementation plan before code starts.
- Keeping the first pass small enough to reason about makes later requirement-by-requirement work tractable.
- The v1 implementation boundary only justified JSON and JSON Text Sequences at the outset. The contained CBOR follow-on now exists as a sibling package, so a general multi-format serializer abstraction would still add complexity beyond the implemented slices.

## Slice Order

The implementation plan is summarized in
[`implementation-slices.md`](implementation-slices.md).
The intended order is core model and envelope first, then QUIC vocabulary
foundation, then transport activity, then migration/recovery state, and only
after that the sequential `JSON Text Sequences` slice.
