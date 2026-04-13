# Incursa.Qlog.Tests

[`Incursa.Qlog.Tests`](../../README.md) is the companion test project scaffold for the Incursa Qlog repository.

## Run

```bash
dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj
```

## Status

- The project now carries the first qlog core-slice tests for contained JSON round-trip and validation behavior.
- The tests are intentionally scoped to the v1 core slice; QUIC vocabulary and sequential JSON Text Sequences remain later work.
- Future test work should still be driven by `specs/verification/qlog`.
