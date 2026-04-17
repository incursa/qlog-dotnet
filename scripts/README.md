# Scripts

This directory holds repo-local helpers for the qlog SpecTrace baseline.

## Entry Points

- [`Invoke-QlogAutopilot.ps1`](./Invoke-QlogAutopilot.ps1): plan, prepare, run, merge, and clean up local Codex worker lanes in separate worktrees for the qlog backlog.
- [`Run-CodexAutopilot.ps1`](./Run-CodexAutopilot.ps1): generic unattended Codex loop runner used by the qlog autopilot worktree lanes.
- [`Test-RequirementHomeCoverage.ps1`](./Test-RequirementHomeCoverage.ps1): audit canonical `REQ-...` identifiers against requirement-home test traits under `tests/`.
- [`Validate-SpecTraceJson.ps1`](./Validate-SpecTraceJson.ps1): validate the canonical qlog SpecTrace JSON corpus against the repo-pinned SpecTrace schema snapshot with trace integrity checks.
- [`Refresh-QlogDraftSources.ps1`](./Refresh-QlogDraftSources.ps1): download the recorded qlog draft text snapshots and rewrite the source manifest.
- [`setup-git-hooks.ps1`](./setup-git-hooks.ps1): configure `git` to use the repo-local `.githooks` directory.
- [`release/Invoke-ReleaseVersioning.ps1`](./release/Invoke-ReleaseVersioning.ps1): compute, apply, and publish a release version bump.
- [`release/validate-public-api-versioning.ps1`](./release/validate-public-api-versioning.ps1): validate the public API shipped baselines for a release tag.
- [`spec-trace/README.md`](./spec-trace/README.md): notes on repo-local SpecTrace helpers.

## Typical Flow

Start from a clean checkout on `main`, then:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-QlogAutopilot.ps1 -Mode plan
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-QlogAutopilot.ps1 -Mode run
```

Useful variants:

- Use `-ParallelLanes 1` to force a single active lane.
- Use `-LaneIds core-capture-runtime` to target a specific lane.
- Use `-Mode merge` or `-Mode cleanup` to manage prepared lanes manually.
- Lane worktrees are created under the repo's sibling `<repo>.worktrees\qlog-autopilot` directory.
- Lane state and transcripts are stored under `.artifacts/qlog-autopilot/`.
