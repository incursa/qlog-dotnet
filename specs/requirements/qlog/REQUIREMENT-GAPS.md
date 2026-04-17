---
workbench:
  type: specification
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/requirements/qlog/REQUIREMENT-GAPS.md
---

# Qlog Requirement Gaps

This ledger tracks open questions, ambiguities, and follow-up decisions for qlog requirement work.

## Open Gaps

- `draft-volatility` is the standing maintenance concern for this baseline. The qlog main-schema and QUIC event documents are both draft-state source material, so the requirement corpus must be revisited when either draft advances.
- `future-h3-events` remains intentionally out of the initial scope. HTTP/3 event mapping may become a later requirement family, but it does not belong in the first qlog baseline.

## Closed Gaps

- `capture-backpressure-policy` is resolved by `REQ-QLOG-CAPTURE-S1-0008`, `REQ-QLOG-CAPTURE-S1-0009`, and `REQ-QLOG-CAPTURE-S1-0010`. The internal dispatcher now defaults to a bounded drop-on-full queue sized from available memory heuristics, while preserving an internal opt-in unbounded mode for callers that explicitly accept the memory trade-off.
- `cbor-serialization-surface` is resolved by [`SPEC-QLOG-CBOR.json`](SPEC-QLOG-CBOR.json), `REQ-QLOG-CBOR-S1-0004`, `REQ-QLOG-CBOR-S1-0005`, `REQ-QLOG-CBOR-S1-0006`, and `REQ-QLOG-CBOR-S1-0007`. The first contained CBOR serializer now ships in the sibling `Incursa.Qlog.Cbor` package, reuses the contained qlog file schema URI, records `application/cbor` as the serialization format media type, and uses `.qlog.cbor` as the canonical file extension.
- `contained-shared-envelope-metadata` is resolved by [`SPEC-QLOG-SINKS.json`](SPEC-QLOG-SINKS.json) and `REQ-QLOG-SINKS-S1-0009`. Contained multi-session sinks now keep the first completed session's non-empty file title and description plus the first-seen value for each file extension member key, while later completed sessions only backfill file-level metadata that was still absent.
- `capture-drop-telemetry-contract` is resolved for the current internal surface. The repository now treats bounded drop-on-full behavior as the safe default without promising a public or semi-public dropped-event telemetry channel.
- `capture-ordering-contract` is resolved by `REQ-QLOG-CAPTURE-S1-0004`. The current dispatcher may preserve per-session enqueue order, but that behavior remains an internal implementation detail and does not create a stable public ordering guarantee.
- `json-seq-first-class` is resolved by the sequential JSON Text Sequences serializer boundary. Keep future adjustments aligned with the draft requirements and the existing contained JSON writer separation.
- `private-schema-policy` is resolved by `REQ-QLOG-MAIN-S8P2-0003`. The repository now treats private and extension event schema URIs as opaque absolute identifiers that must round-trip without built-in registry support.
- `public-write-surface` is resolved by `REQ-QLOG-CAPTURE-S1-0007`. The public package surface stays centered on the retained qlog model and serializer entry points while capture and sink coordination remain internal.
- `serializer-stream-surface` is resolved by `REQ-QLOG-MAIN-S9-0001` and `REQ-QLOG-MAIN-S11P1-0001`. The core package keeps explicit string and `Stream` serializer entry points for both first-class qlog formats so callers can write directly to files, streams, or buffers without intermediate text materialization.

## How To Use

- Add a gap here before implementation whenever the draft text leaves more than one plausible interpretation.
- Keep the note short and actionable.
- Reference the owning `SPEC-...` file and the follow-up artifact if one exists.
