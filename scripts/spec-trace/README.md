# SpecTrace Scripts

This directory holds repo-local helpers for canonical JSON-authored SpecTrace artifacts.

## Commands

- [`Validate-SpecTraceJson.ps1`](../Validate-SpecTraceJson.ps1): validate the canonical qlog SpecTrace JSON corpus against the published SpecTrace schema.

## Notes

- Canonical SpecTrace artifacts are authored in `.json`, and the repository does not keep sibling canonical `.md` companions for those families.
- The local validation script pulls the published model schema from `incursa/spec-trace` instead of checking in a repo-local copy.
- The qlog repository does not yet carry the heavier generation and lane-running scripts from `quic-dotnet`.
