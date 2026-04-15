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

## Notes

- Keep the design layer focused on satisfaction paths, invariants, and tradeoffs.
- The architecture stays draft-aware and keeps sequential `JSON Text Sequences` as a bounded serializer slice over the shared qlog model.
