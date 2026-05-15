# Qlog Implementation Slices

This note groups the recorded qlog requirement baseline into a bounded v1
implementation sequence.
The recorded source drafts remain work-in-progress Internet-Drafts, so the
slice boundaries are planning guidance rather than frozen protocol truth.

## Slice 1: Core Model And Envelope

- Requirements:
  - `REQ-QLOG-MAIN-S3-0001`
  - `REQ-QLOG-MAIN-S4-0001`
  - `REQ-QLOG-MAIN-S6-0001`
  - `REQ-QLOG-MAIN-S7-0001`
  - `REQ-QLOG-MAIN-S8P1-0001`
  - `REQ-QLOG-MAIN-S8P1-0002`
  - `REQ-QLOG-MAIN-S8P2-0001`
  - `REQ-QLOG-MAIN-S8P2-0002`
- Implementation focus:
  - qlog file and trace envelope contracts
  - event identity and common fields
  - serialization-neutral object model
  - writer/sink boundary for JSON-compatible output
  - unknown-field and extension preservation
- Planned verification:
  - `VER-QLOG-CORE-0001`

## Slice 2: QUIC Vocabulary Foundation

- Requirements:
  - `REQ-QLOG-QUIC-S2-0001`
  - `REQ-QLOG-QUIC-S4-0001`
  - `REQ-QLOG-QUIC-S5P1-0001`
- Implementation focus:
  - QUIC namespace and schema registration
  - connection and path lifecycle mapping
  - version negotiation and transport-parameter events
- Planned verification:
  - `VER-QLOG-QUIC-0001`

## Slice 3: QUIC Transport Activity

- Requirements:
  - `REQ-QLOG-QUIC-S5P2-0001`
  - `REQ-QLOG-QUIC-S5P3-0001`
- Implementation focus:
  - packet and UDP datagram activity
  - stream and datagram movement
  - event-shape snapshots for packet/data-path transitions
- Planned verification:
  - `VER-QLOG-QUIC-0002`

## Slice 4: QUIC State And Recovery

- Requirements:
  - `REQ-QLOG-QUIC-S6-0001`
  - `REQ-QLOG-QUIC-S7-0001`
- Implementation focus:
  - connection ID and migration state
  - key lifecycle events
  - recovery parameters, metrics, and congestion state
- Planned verification:
  - `VER-QLOG-QUIC-0003`

## Slice 5: Sequential JSON Text Sequences

- Requirements:
  - `REQ-QLOG-MAIN-S5-0001`
  - `REQ-QLOG-MAIN-S11P2-0001`
- Implementation focus:
  - sequential qlog files
  - `JSON Text Sequences` field casing rules
  - any serializer behavior that should stay separate from the contained JSON writer
- Planned verification:
  - `VER-QLOG-SEQUENTIAL-0001`

## Slice 6: Contained CBOR Serialization

- Requirements:
  - `REQ-QLOG-CBOR-S1-0001`
  - `REQ-QLOG-CBOR-S1-0002`
  - `REQ-QLOG-CBOR-S1-0003`
  - `REQ-QLOG-CBOR-S1-0004`
  - `REQ-QLOG-CBOR-S1-0005`
  - `REQ-QLOG-CBOR-S1-0006`
  - `REQ-QLOG-CBOR-S1-0007`
- Implementation focus:
  - sibling `Incursa.Qlog.Cbor` package boundary
  - contained qlog CBOR serializer over the retained model
  - stream-first binary output
  - contained artifact policy using `urn:ietf:params:qlog:file:contained`, `application/cbor`, and `.qlog.cbor`
- Planned verification:
  - `VER-QLOG-CBOR-0001`

## Slice 7: Import And Rehydration

- Requirements:
  - `REQ-QLOG-IMPORT-S1-0001`
  - `REQ-QLOG-IMPORT-S1-0002`
  - `REQ-QLOG-IMPORT-S1-0003`
  - `REQ-QLOG-IMPORT-S1-0004`
  - `REQ-QLOG-IMPORT-S1-0005`
- Implementation focus:
  - sibling `Incursa.Qlog.Import` package boundary
  - contained JSON hydration into the retained qlog model
  - sequential JSON Text Sequences hydration into the retained qlog model
  - string and Stream import entry points with format dispatch
  - unknown-member and opaque-identifier preservation
- Planned verification:
  - `VER-QLOG-IMPORT-0001`

## Slice 8: Contained CBOR Import

- Requirements:
  - `REQ-QLOG-IMPORT-S1-0006`
- Implementation focus:
  - contained CBOR hydration inside the sibling `Incursa.Qlog.Import` package
  - binary payload dispatch and retained-model conversion
  - CBOR trace component classification and opaque-value preservation
- Planned verification:
  - `VER-QLOG-IMPORT-0002`

## Slice 9: Main Schema Parity Refresh

- Requirements:
  - `REQ-QLOG-MAIN-S1P2-0001`
  - `REQ-QLOG-MAIN-S3P1-0001`
  - `REQ-QLOG-MAIN-S8P3-0001`
  - `REQ-QLOG-MAIN-S9-0002`
  - `REQ-QLOG-MAIN-S9-0003`
  - `REQ-QLOG-MAIN-S10-0001`
  - `REQ-QLOG-MAIN-S11P1-0002`
  - `REQ-QLOG-MAIN-S11P3-0001`
  - `REQ-QLOG-MAIN-S12P1-0001`
  - `REQ-QLOG-MAIN-S13-0001`
  - `REQ-QLOG-MAIN-S14-0001`
- Implementation focus:
  - latest main-schema guidance not captured by the original draft-13 baseline
  - retained-model preservation behavior that already exists in the serializer and importer
  - QUIC raw-value shapes for length-only, data-only, and truncated raw data
  - planned schema-description, file-generation, and capture-policy surfaces that must stay visible but not be claimed complete
- Planned verification:
  - `VER-QLOG-MAIN-0001`

## Review Items

- The qlog drafts remain draft-state, so event names, section references, and schema URIs may drift.
- The main-schema parity refresh references the QUICWG latest preview, while the local manifest remains pinned to the draft-13 source snapshot until a deliberate source refresh is performed.
- Import and replay stay separate concerns; the reader package should hydrate retained qlog models without absorbing transport execution logic.
- The import package now covers contained JSON, sequential JSON Text Sequences, and contained CBOR hydration in the sibling library.
- The QUIC fixture hydration tests consume the sibling importer from the QUIC test project, while the explicit serializer round-trip proof remains in `tests/Incursa.Qlog.Import.Tests`.
