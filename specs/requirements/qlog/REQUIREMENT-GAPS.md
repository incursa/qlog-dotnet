---
workbench:
  type: specification
  workItems: []
  codeRefs: []
  pathHistory: []
  path: /specs/requirements/qlog/REQUIREMENT-GAPS.md
---

# Qlog Requirement Gaps

This ledger tracks open questions, ambiguities, and follow-up decisions for qlog requirement work.

## Open Gaps

- `draft-volatility` is the standing maintenance concern for this baseline. The qlog main-schema and QUIC event documents are both draft-state source material, so the requirement corpus must be revisited when either draft advances.
- `json-seq-first-class` is still a decision point for implementation planning. The current baseline includes JSON Text Sequences requirements, but the repository still needs to decide how eagerly that output surface should appear in the first implementation slice.
- `future-h3-events` remains intentionally out of the initial scope. HTTP/3 event mapping may become a later requirement family, but it does not belong in the first qlog baseline.
- `private-schema-policy` still needs a concrete implementation rule. The drafts allow private and extension event schemas, but the repository still needs to decide how strict the initial validation and registration support should be.

## How To Use

- Add a gap here before implementation whenever the draft text leaves more than one plausible interpretation.
- Keep the note short and actionable.
- Reference the owning `SPEC-...` file and the follow-up artifact if one exists.
