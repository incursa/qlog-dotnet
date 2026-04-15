# SpecTrace Scripts

This directory holds repo-local helpers for canonical JSON-authored SpecTrace artifacts.

## Commands

- [`Validate-SpecTraceJson.ps1`](../Validate-SpecTraceJson.ps1): validate the canonical qlog SpecTrace JSON corpus against the repo-pinned SpecTrace schema snapshot.

## Notes

- Canonical SpecTrace artifacts are authored in `.json`, and the repository does not keep sibling canonical `.md` companions for those families.
- The validator prefers the repo-local `model.schema.json` snapshot so CI and clean clones use the same contract as local development.
- If the repo-local snapshot is absent, the validator falls back to a sibling `spec-trace` checkout and then the published schema URI.
- The qlog repository does not yet carry the heavier generation and lane-running scripts from `quic-dotnet`.
