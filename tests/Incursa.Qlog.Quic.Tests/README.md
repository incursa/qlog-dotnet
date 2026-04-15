# Incursa.Qlog.Quic.Tests

[`Incursa.Qlog.Quic.Tests`](../../README.md) is the requirement-homed test project for the `Incursa.Qlog.Quic` package.

## Run

```bash
dotnet test tests/Incursa.Qlog.Quic.Tests/Incursa.Qlog.Quic.Tests.csproj
```

## Scope

- Covers the QUIC lifecycle, negotiation, transport activity, and state/recovery slices traced in `specs/verification/qlog`.
- Leaves core qlog coverage in `tests/Incursa.Qlog.Tests`.
- Extends only when the linked verification artifacts expand or change.
