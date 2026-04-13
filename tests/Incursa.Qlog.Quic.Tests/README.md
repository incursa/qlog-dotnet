# Incursa.Qlog.Quic.Tests

[`Incursa.Qlog.Quic.Tests`](../../README.md) is the dedicated requirement-homed test project for the QUIC qlog slices.

## Run

```bash
dotnet test tests/Incursa.Qlog.Quic.Tests/Incursa.Qlog.Quic.Tests.csproj
```

## Status

- The project carries requirement-homed tests for the QUIC lifecycle / negotiation slice, the QUIC transport activity slice, and the QUIC migration / recovery slice.
- Core qlog coverage remains in `tests/Incursa.Qlog.Tests`.
- Future test work should still be driven by `specs/verification/qlog`.
