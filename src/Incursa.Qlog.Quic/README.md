# Incursa.Qlog.Quic

[`Incursa.Qlog.Quic`](../../README.md) is the QUIC vocabulary and mapping package that builds on the core qlog model in `Incursa.Qlog`.

## Install

```bash
dotnet add package Incursa.Qlog.Quic
```

## Status

- The QUIC lifecycle, negotiation, packet/UDP activity, stream/datagram movement, and migration/recovery slices now live in this package.
- The package depends on `Incursa.Qlog` for the core qlog model, contained JSON serializer, and generic value infrastructure.
- Future QUIC work should still be driven by canonical requirements in `specs/requirements/qlog`.
