# SpecTrace Prep

This note defines how `Incursa.Qlog` will move from draft text to traceable implementation work.

## Canonical Homes

- Requirements live under [`../specs/requirements/qlog/README.md`](../specs/requirements/qlog/README.md).
- Requirement gaps live in [`../specs/requirements/qlog/REQUIREMENT-GAPS.md`](../specs/requirements/qlog/REQUIREMENT-GAPS.md).
- Architecture and design artifacts live under [`../specs/architecture/qlog/README.md`](../specs/architecture/qlog/README.md).
- Work items live under [`../specs/work-items/qlog/README.md`](../specs/work-items/qlog/README.md).
- Verification artifacts live under [`../specs/verification/qlog/README.md`](../specs/verification/qlog/README.md).
- Derived provenance and boundary notes live under [`../specs/generated/qlog/README.md`](../specs/generated/qlog/README.md).
- Repo-wide validation lives in [`../scripts/Validate-SpecTraceJson.ps1`](../scripts/Validate-SpecTraceJson.ps1).

## Order Of Operations

1. Start from the relevant draft section or qlog concern.
2. Check for an existing owning `SPEC-...` file and any open gap in [`REQUIREMENT-GAPS.md`](../specs/requirements/qlog/REQUIREMENT-GAPS.md).
3. Record the provenance and the initial boundary before implementation work begins.
4. Refresh the draft snapshot manifest with [`../scripts/Refresh-QlogDraftSources.ps1`](../scripts/Refresh-QlogDraftSources.ps1) whenever the source drafts move.
5. Turn the requirement baseline into a bounded v1 implementation sequence in [`../specs/generated/qlog/implementation-slices.md`](../specs/generated/qlog/implementation-slices.md).
6. Write or revise canonical requirements in `specs/requirements/qlog`.
7. Add architecture notes when the satisfaction path, invariants, or tradeoffs need explanation.
8. Add or update the linked work item only after the requirement text is stable enough to trace.
9. Write the verification artifact early enough that the proof burden is explicit before coding starts.
10. Implement.
11. Validate the canonical JSON artifacts before treating the slice as ready for implementation work.

## Notes

- The draft source set is split between the core main-schema material and the QUIC event schema material, so the repository keeps those as separate `SPEC-...` files.
- The repository now carries a local Codex autopilot entrypoint for worktree-based backlog execution because the capture, sink, and verification workload justifies it. Keep that automation subordinate to the canonical requirements, tests, and runtime state.
