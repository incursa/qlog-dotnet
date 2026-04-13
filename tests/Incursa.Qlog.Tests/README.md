# Incursa.Qlog.Tests

[`Incursa.Qlog.Tests`](../../README.md) is the companion test project scaffold for the Incursa Qlog repository.

## Run

```bash
dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj
```

## Status

- The project now carries requirement-homed tests for the contained JSON core slice.
- QUIC requirement-homed tests live in `tests/Incursa.Qlog.Quic.Tests`.
- Requirement-homed tests now cover the sequential JSON Text Sequences slice.
- Future test work should still be driven by `specs/verification/qlog`.
