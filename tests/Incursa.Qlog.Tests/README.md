# Incursa.Qlog.Tests

[`Incursa.Qlog.Tests`](../../README.md) is the requirement-homed test project for the core `Incursa.Qlog` package.

## Run

```bash
dotnet test tests/Incursa.Qlog.Tests/Incursa.Qlog.Tests.csproj
```

## Scope

- Covers the core qlog model and the contained and sequential JSON serializer behavior traced in `specs/verification/qlog`.
- Keeps QUIC-specific coverage in `tests/Incursa.Qlog.Quic.Tests`.
- Extends only when the linked verification artifacts expand or change.
