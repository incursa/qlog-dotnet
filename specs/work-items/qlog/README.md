---
workbench:
  type: work_item
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/work-items/qlog/README.md
---

# Qlog Work Items

This directory holds qlog implementation planning artifacts.
Each canonical artifact is authored in `.json`.

## Artifacts

- [`WI-QLOG-BASELINE-0001.json`](WI-QLOG-BASELINE-0001.json): umbrella qlog v1 implementation plan
- [`WI-QLOG-CBOR-0001.json`](WI-QLOG-CBOR-0001.json): contained CBOR serialization work item
- [`WI-QLOG-CAPTURE-0001.json`](WI-QLOG-CAPTURE-0001.json): internal capture and dispatch work item
- [`WI-QLOG-CORE-0001.json`](WI-QLOG-CORE-0001.json): contained core model and JSON writer work item
- [`WI-QLOG-QUIC-0001.json`](WI-QLOG-QUIC-0001.json): QUIC vocabulary foundation work item
- [`WI-QLOG-QUIC-0002.json`](WI-QLOG-QUIC-0002.json): QUIC transport activity work item
- [`WI-QLOG-QUIC-0003.json`](WI-QLOG-QUIC-0003.json): QUIC migration and recovery work item
- [`WI-QLOG-SEQUENTIAL-0001.json`](WI-QLOG-SEQUENTIAL-0001.json): sequential JSON Text Sequences work item
- [`WI-QLOG-SINKS-0001.json`](WI-QLOG-SINKS-0001.json): built-in file and stream sink work item

## Notes

- Keep work items descriptive of delivery work, not of the normative requirement text itself.
- The sequential qlog slice uses a bounded serializer and envelope boundary over the contained JSON model.
- The CBOR work item records the implemented sibling package and contained artifact policy.
