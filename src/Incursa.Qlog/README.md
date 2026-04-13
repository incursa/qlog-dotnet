# Incursa.Qlog

[`Incursa.Qlog`](../../README.md) is the packable library root for qlog-related models, abstractions, and tooling used for contained JSON qlog work.

## Install

```bash
dotnet add package Incursa.Qlog
```

## Status

- The qlog core model and contained JSON baseline now exist for the v1 core slice.
- The contained JSON serializer and sequential JSON Text Sequences serializer now coexist as separate format boundaries.
- Unknown file, trace, common-field, vantage-point, reference-time, and event members are preserved through explicit extension data.
- The core package stays free of QUIC-specific vocabulary; that surface now lives in `Incursa.Qlog.Quic`.
- Future implementation should still be driven by canonical requirements in `specs/requirements/qlog`.
