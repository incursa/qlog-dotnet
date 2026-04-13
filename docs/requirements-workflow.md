# Qlog Requirements Workflow

This note captures the working order for draft-driven qlog work in `Incursa.Qlog`. It is guidance, not protocol design.

## Order Of Operations

1. Capture the source material.
   - Use the draft main-schema and draft quic-events documents as the current source set.
   - Record provenance and the first-scope boundary in `specs/generated/qlog/source-provenance.md` and `specs/generated/qlog/scope-boundary.md`.
2. Draft the smallest stable requirement slice.
   - Keep the split between `SPEC-QLOG-MAIN` and `SPEC-QLOG-QUIC`.
   - Keep normative statements separate from notes, examples, and rationale.
3. Add architecture only after the requirement slice is stable enough to design against.
4. Add work items after the requirement and design shape are clear.
5. Add verification artifacts before implementation reaches review.
6. Implement code, tests, serializers, and adapters in the same slice when they affect the same behavior.
7. Revisit the provenance note whenever the source drafts update.

## Canonical Paths

- Requirements: [`../specs/requirements/qlog/README.md`](../specs/requirements/qlog/README.md)
- Architecture: [`../specs/architecture/qlog/README.md`](../specs/architecture/qlog/README.md)
- Work items: [`../specs/work-items/qlog/README.md`](../specs/work-items/qlog/README.md)
- Verification: [`../specs/verification/qlog/README.md`](../specs/verification/qlog/README.md)
- Generated outputs: [`../specs/generated/qlog/README.md`](../specs/generated/qlog/README.md)

## Notes

- The baseline is intentionally draft-based and should not be treated as frozen protocol truth.
- Keep the first implementation pass focused on the core qlog model and the QUIC event vocabulary that the source drafts already define.
