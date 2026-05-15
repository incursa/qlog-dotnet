---
workbench:
  type: verification
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/verification/qlog/README.md
---

# Qlog Verification

This directory holds the qlog verification artifacts for the implementation plan.
Each canonical artifact is authored in `.json`.

## Artifacts

- [`VER-QLOG-BASELINE-0001.json`](VER-QLOG-BASELINE-0001.json): umbrella qlog v1 verification plan
- [`VER-QLOG-CBOR-0001.json`](VER-QLOG-CBOR-0001.json): contained CBOR serialization verification plan
- [`VER-QLOG-CAPTURE-0001.json`](VER-QLOG-CAPTURE-0001.json): internal capture and dispatch verification shell
- [`VER-QLOG-IMPORT-0001.json`](VER-QLOG-IMPORT-0001.json): sibling import and rehydration verification shell
- [`VER-QLOG-IMPORT-0002.json`](VER-QLOG-IMPORT-0002.json): contained CBOR import verification shell
- [`VER-QLOG-MAIN-0001.json`](VER-QLOG-MAIN-0001.json): main-schema parity refresh verification plan
- [`VER-QLOG-CORE-0001.json`](VER-QLOG-CORE-0001.json): contained core model verification shell
- [`VER-QLOG-QUIC-0001.json`](VER-QLOG-QUIC-0001.json): QUIC vocabulary foundation verification shell
- [`VER-QLOG-QUIC-0002.json`](VER-QLOG-QUIC-0002.json): QUIC transport activity verification shell
- [`VER-QLOG-QUIC-0003.json`](VER-QLOG-QUIC-0003.json): QUIC migration and recovery verification shell
- [`VER-QLOG-QUIC-0004.json`](VER-QLOG-QUIC-0004.json): QUIC fixture hydration verification shell
- [`VER-QLOG-SEQUENTIAL-0001.json`](VER-QLOG-SEQUENTIAL-0001.json): sequential JSON Text Sequences verification artifact
- [`VER-QLOG-SINKS-0001.json`](VER-QLOG-SINKS-0001.json): built-in file and stream sink verification shell

## Notes

- Keep verification artifacts homogeneous in status.
- Split artifacts when the requirements they cover do not share the same outcome.
- The sequential verification artifact tracks the serializer boundary for the sequential format.
- The CBOR verification artifact tracks the implemented sibling serializer boundary and the selected artifact-metadata policy.
- The import verification artifacts track the sibling reader boundary and hydration behavior for the repository's standard output.
- The QUIC fixture hydration verification artifact tracks consumer-side import usage in the QUIC test project and points back to the import package tests for the explicit round-trip proof.
- The main-schema parity verification artifact records executable requirement-home proof for implemented guidance and skipped placeholders for intentionally planned surfaces.
