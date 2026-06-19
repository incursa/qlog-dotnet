---
title: "Qlog Documentation"
---

# Qlog Documentation

This directory holds the source-authored documentation for `Incursa.Qlog`. The content is mirrored into `incursa-docs` for publication, so update the source files here rather than the generated copy.

## Start Here

- [`../README.md`](../README.md): repository purpose, package surface, validation commands, release/versioning, security notes, and known gaps.
- [`maintainer-readiness.md`](maintainer-readiness.md): repository purpose, package surface, architecture, validation commands, release/versioning expectations, current readiness status, and known gaps.
- [`requirements-workflow.md`](requirements-workflow.md): working order for requirement-first qlog changes.
- [`spec-trace-prep.md`](spec-trace-prep.md): notes for preparing the local SpecTrace corpus.

## Authoritative Surfaces

- Package code lives under `src/`.
- Package-level consumer notes live in each package README under `src/<PackageName>/`.
- Canonical requirements, architecture, work items, and verification artifacts live under `specs/`.
- Tests under `tests/` are requirement-homed and should be updated with the owning requirement or verification artifact.
- Mirrored docs under `incursa-docs` are generated output and are not edited in this repository group.
