---
workbench:
  type: specification
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/requirements/qlog/README.md
---

# Qlog Requirements

This directory holds the qlog requirement slice for the repository.
Each canonical artifact is authored in `.json`.

## Artifacts

- [`SPEC-QLOG-MAIN.json`](SPEC-QLOG-MAIN.json): canonical draft main-schema requirement source
- [`SPEC-QLOG-QUIC.json`](SPEC-QLOG-QUIC.json): canonical draft QUIC event requirement source
- [`REQUIREMENT-GAPS.md`](REQUIREMENT-GAPS.md): the local gap ledger

## Notes

- The draft source material is split between the qlog main-schema draft and the QUIC event draft.
- Provenance and the first-scope boundary live in [`../../generated/qlog/source-provenance.md`](../../generated/qlog/source-provenance.md) and [`../../generated/qlog/scope-boundary.md`](../../generated/qlog/scope-boundary.md).
- The v1 implementation sequence is summarized in [`../../generated/qlog/implementation-slices.md`](../../generated/qlog/implementation-slices.md).
- Keep future qlog work traceable to one of the `SPEC-...` files before implementation.
