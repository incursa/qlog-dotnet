# Incursa.Qlog

[`Incursa.Qlog`](../../README.md) is the packable library root for qlog-related models, abstractions, and tooling used for QUIC/qlog diagnostics and structured event work.

## Install

```bash
dotnet add package Incursa.Qlog
```

## Status

- The qlog core model and contained JSON baseline now exist for the v1 core slice.
- The current implementation is intentionally contained-only; sequential JSON Text Sequences remain a later slice.
- Unknown file, trace, common-field, vantage-point, reference-time, and event members are preserved through explicit extension data.
- The first `Incursa.Qlog.Quic` slice now covers draft QUIC lifecycle and negotiation events on top of the generic `QlogEvent` model.
- QUIC traces should advertise the draft schema URI `urn:ietf:params:qlog:events:quic-12` through `QlogQuicEvents.RegisterDraftSchema`.
- Future implementation should still be driven by canonical requirements in `specs/requirements/qlog`.
