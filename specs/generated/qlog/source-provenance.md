# Qlog Source Provenance

This note records the draft source material used to author the initial qlog baseline in `Incursa.Qlog` and `Incursa.Qlog.Quic`.

## Source Documents

- `draft-ietf-quic-qlog-main-schema-13`
  - Recorded draft main-schema source used for the qlog core model.
  - Retrieved from the IETF datatracker on 2026-04-13.
  - Relevant sections for this baseline: file envelopes, trace metadata, common fields, event schemas, and JSON / JSON Text Sequence serialization.
  - Canonical URL: <https://datatracker.ietf.org/doc/html/draft-ietf-quic-qlog-main-schema-13>
  - Local text snapshot: [`source-docs/draft-ietf-quic-qlog-main-schema-13.txt`](source-docs/draft-ietf-quic-qlog-main-schema-13.txt)
- `draft-ietf-quic-qlog-quic-events-12`
  - Recorded draft QUIC event source used for the qlog QUIC vocabulary.
  - Retrieved from the IETF datatracker on 2026-04-13.
  - Relevant sections for this baseline: QUIC event namespace, connection lifecycle, transport parameters, packet activity, stream movement, migration, key lifecycle, and recovery/congestion events.
  - Canonical URL: <https://datatracker.ietf.org/doc/html/draft-ietf-quic-qlog-quic-events-12>
  - Local text snapshot: [`source-docs/draft-ietf-quic-qlog-quic-events-12.txt`](source-docs/draft-ietf-quic-qlog-quic-events-12.txt)

## Extraction Approach

- The initial corpus was extracted manually from the draft section headings and normative statements.
- No parser, code generator, or coverage lane was introduced for this first pass.
- The source snapshots and hashes are tracked in [`source-manifest.json`](source-manifest.json) so later refreshes can confirm the same draft revisions were used.
- The extraction stayed intentionally narrow so the repository could keep a clean split between the core qlog model and the QUIC event mapping layer.

## Draft Caveats

- Both source documents are work-in-progress Internet-Drafts.
- Their section numbering, event names, and URI details may still change.
- Revisit the qlog requirements corpus whenever the upstream draft versions change.
- Refresh the local text snapshots with [`../../../scripts/Refresh-QlogDraftSources.ps1`](../../../scripts/Refresh-QlogDraftSources.ps1) when the upstream drafts advance.
