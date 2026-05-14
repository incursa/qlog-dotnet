# Incursa.Qlog.Import.Tests

[`Incursa.Qlog.Import.Tests`](../../README.md) is the requirement-homed test project for the `Incursa.Qlog.Import` package.

## Run

```bash
dotnet test tests/Incursa.Qlog.Import.Tests/Incursa.Qlog.Import.Tests.csproj
```

## Scope

- Covers the sibling qlog import and rehydration surface traced in `specs/verification/qlog`, including contained CBOR hydration.
- Leaves writer-side coverage in `tests/Incursa.Qlog.Tests`.
- Extends only when the linked verification artifact expands or changes.
