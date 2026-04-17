---
workbench:
  type: architecture
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/architecture/qlog/README.md
---

# Qlog Architecture

This directory holds the qlog design artifact(s) for the implementation plan.
Each canonical artifact is authored in `.json`.

## Artifacts

- [`ARC-QLOG-BASELINE-0001.json`](ARC-QLOG-BASELINE-0001.json): canonical qlog v1 architecture and implementation boundary
- [`ARC-QLOG-CBOR-0001.json`](ARC-QLOG-CBOR-0001.json): contained CBOR serialization architecture boundary
- [`ARC-QLOG-CAPTURE-0001.json`](ARC-QLOG-CAPTURE-0001.json): internal capture and dispatch architecture boundary
- [`ARC-QLOG-SINKS-0001.json`](ARC-QLOG-SINKS-0001.json): built-in file and stream sink architecture boundary

## Notes

- Keep the design layer focused on satisfaction paths, invariants, and tradeoffs.
- The architecture stays draft-aware and keeps sequential `JSON Text Sequences` as a bounded serializer slice over the shared qlog model.
- The CBOR slice stays separate from sink integration even after the sibling package and artifact-metadata policy are explicit.
