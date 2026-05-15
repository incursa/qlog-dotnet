# Incursa.Qlog.Quic.Tests

[`Incursa.Qlog.Quic.Tests`](../../README.md) is the requirement-homed test project for the `Incursa.Qlog.Quic` package.

## Run

```bash
dotnet test tests/Incursa.Qlog.Quic.Tests/Incursa.Qlog.Quic.Tests.csproj
```

## Scope

- Covers the QUIC lifecycle, negotiation, transport activity, and state/recovery slices traced in `specs/verification/qlog`.
- Covers recorded qlog fixture hydration through the sibling `Incursa.Qlog.Import` package without adding import/parsing dependencies to `Incursa.Qlog.Quic` production code.
- Leaves core qlog coverage in `tests/Incursa.Qlog.Tests`.
- Leaves explicit importer round-trip coverage for contained JSON, sequential JSON Text Sequences, and contained CBOR in `tests/Incursa.Qlog.Import.Tests`.
- Extends only when the linked verification artifacts expand or change.

## Fixture Import

Recorded fixtures live under `Fixtures/Artifacts` and are copied to the test output directory. Use `QlogFixtureLoader.LoadQlog(...)` for contained JSON, sequential JSON Text Sequences, and auto-detected contained CBOR fixtures:

```csharp
QlogFile file = QlogFixtureLoader.LoadQlog("Fixtures", "Artifacts", "captured-quic-contained.qlog.json");
```

When a fixture is known to be contained CBOR, use the explicit helper so the test documents that contract:

```csharp
QlogFile file = QlogFixtureLoader.LoadContainedCborQlog("Fixtures", "Artifacts", "fixture.qlog.cbor");
```

Assertions should inspect hydrated `QlogFile`, `QlogTrace`, `QlogEvent`, `QlogTraceError`, `QlogVantagePoint`, common fields, event data, and extension data. Do not compare raw JSON text in these fixture tests, and keep replay execution out of scope.

Use `QlogFixtureAssertions.AssertJsonEquivalent(...)` for nested opaque values that should be compared semantically instead of by raw text. Refresh the recorded fixtures whenever the recorded qlog draft revisions change; the draft-volatility gap in `specs/requirements/qlog/REQUIREMENT-GAPS.md` is the reminder to keep these inputs current.
