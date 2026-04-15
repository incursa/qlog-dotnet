# Scripts

This directory holds repo-local helpers for the qlog SpecTrace baseline.

## Current Entry Points

- [`Validate-SpecTraceJson.ps1`](./Validate-SpecTraceJson.ps1): validate the canonical qlog SpecTrace JSON corpus against the repo-pinned SpecTrace schema snapshot with trace integrity checks.
- [`Refresh-QlogDraftSources.ps1`](./Refresh-QlogDraftSources.ps1): download the recorded qlog draft text snapshots and rewrite the source manifest.
- [`setup-git-hooks.ps1`](./setup-git-hooks.ps1): configure `git` to use the repo-local `.githooks` directory.
- [`release/Invoke-ReleaseVersioning.ps1`](./release/Invoke-ReleaseVersioning.ps1): compute, apply, and publish a release version bump.
- [`release/validate-public-api-versioning.ps1`](./release/validate-public-api-versioning.ps1): validate the public API shipped baselines for a release tag.
- [`spec-trace/README.md`](./spec-trace/README.md): notes on repo-local SpecTrace helpers.
