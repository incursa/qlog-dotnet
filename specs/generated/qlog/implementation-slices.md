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

## Review Items

- `private-schema-policy` still needs a concrete implementation rule.
- The qlog drafts remain draft-state, so event names, section references, and schema URIs may drift.
