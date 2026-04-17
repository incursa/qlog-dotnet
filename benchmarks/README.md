# Benchmarks

This directory contains permanent BenchmarkDotNet suites for `Incursa.Qlog`.

## Included Suites

- `QlogStreamDataMovedBenchmarks`: compares a representative payload-move step against the same step plus `QlogQuicEvents.CreateStreamDataMoved`.
- `QlogStreamDataMovedSequentialBenchmarks`: compares the same payload-move step against the current public sequential JSON Text Sequences path.
- `QlogStreamDataMovedCaptureBenchmarks`: compares the same payload-move step against the internal capture coordinator plus the built-in stream sink completion path.

These suites are comparative baselines. They are intended to quantify the incremental burden of adding qlog on top of representative transport work, not to claim an absolute QUIC throughput budget.

## Run

```bash
dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Dry --filter "*QlogStreamDataMovedBenchmarks*"
dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Dry --filter "*QlogStreamDataMovedSequentialBenchmarks*"
dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Dry --filter "*QlogStreamDataMovedCaptureBenchmarks*"
dotnet run -c Release --project benchmarks/Incursa.Qlog.Benchmarks.csproj -- --job Short --filter "*QlogStreamDataMoved*"
```

Use `--filter` to narrow to a subset of benchmarks while iterating locally.
