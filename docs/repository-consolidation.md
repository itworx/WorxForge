# Repository Consolidation: WorxForge-FE → WorxForge monorepo

This document records the merge of the `WorxForge-FE` repository into `WorxForge-BE`
to form a single **WorxForge** monorepo, preserving the full Git history of both.

## Summary

| | |
|---|---|
| Migration date | 2026-08-13 |
| Target repository | `itworx/WorxForge-BE` (to be renamed `itworx/WorxForge`) |
| Source repository | `itworx/WorxForge-FE` (to be archived, not deleted) |
| Strategy | `git merge --allow-unrelated-histories` (no history rewrite) |
| Frontend location | `worxforge-web/` → `src/WorxForge.UI/` |

## Baseline SHAs (frozen at migration time)

| Ref | SHA |
|---|---|
| WorxForge-BE `main` | `e7d70618e6025b64fdacea1d71f000d360239580` |
| WorxForge-FE `main` | `a4e9838131afa4a04eda246946343e881247b23d` |

Backups (mirror clones, kept read-only, outside the repo):
`WorxForge-BE-backup.git`, `WorxForge-FE-backup.git`.

## Migration commits (branch: `chore/merge-fe-monorepo`)

| # | Commit | Purpose |
|---|---|---|
| 1 | `8d8a3f254593a0cd32ff617297d5796557ff7cb3` | Merge FE into monorepo (2-parent merge; parents = BE `e7d7061`, FE `a4e9838`); resolves 5 root-file conflicts |
| 2 | `f4c2e327dd7421318be95cc90e3b2330107bdde7` | Relocate frontend `worxforge-web/` → `src/WorxForge.UI/` (pure rename, 140 files @ 100%) |
| 3 | _(this commit)_ | Update path references (README, setup scripts), scoped `.editorconfig` rule, add this log |

## Repository facts at migration time

| | WorxForge-BE | WorxForge-FE |
|---|---|---|
| Commits on `main` | 57 (7 merge commits) | 42 (5 merge commits) |
| Branches | 30 | 27 |
| Tags | 0 | 0 |
| GitHub Releases | 0 | 0 |
| Open PRs / Issues | 0 / 0 | 0 / 0 |
| CI/CD workflows | none | none |
| Branch protection / rulesets | none | none |
| Secrets / variables / webhooks | none | none |
| Environments | `copilot` (auto, empty) | none |

## Conflicts encountered and resolutions

All conflicts were `add/add` on 5 root files (application code — `src/` vs `worxforge-web/` — never collided):

| File | Resolution |
|---|---|
| `README.md` | Rewritten as a unified monorepo README covering backend + frontend |
| `.gitignore` | BE version (a strict superset of FE's); Vite-specific rules remain in `src/WorxForge.UI/.gitignore` |
| `.editorconfig` | BE .NET rules + a frontend-scoped section (TS/JS → LF, 2-space) + `src/WorxForge.UI/**` LF rule |
| `setup.ps1` | Unified orchestrator: backend + frontend, `-BackendOnly` / `-FrontendOnly` / `-NonInteractive` / `-SkipBuild` / `-StartDevServer` |
| `setup.sh` | Unified orchestrator: backend + frontend, `--backend-only` / `--frontend-only` / `--non-interactive` / `--skip-build` / `--start-dev-server` |

Left intentionally unchanged (not path references): npm package `name` (`worxforge-web`) in
`src/WorxForge.UI/package.json` and `package-lock.json`, and the `<title>` in `index.html`.

## History verification

- Merge commit has two parents: BE `e7d7061` and FE `a4e9838`.
- `git merge-base --is-ancestor a4e9838131... HEAD` → success (FE `main` reachable from monorepo HEAD).
- `git merge-base --is-ancestor e7d7061... HEAD` → success (BE `main` reachable).
- `git diff e7d7061 HEAD -- src/ Directory.*.props global.json .config migration.sql` → empty (backend byte-identical).
- `git log --follow src/WorxForge.UI/...` traverses the rename into original FE commits.

## Build / test results

- **Frontend** (`src/WorxForge.UI`, Node v24.18.1 / npm 11.16.0): see PR / review notes.
- **Backend**: not built locally — the pinned SDK `10.0.302` was not installed on the migration
  machine (only `10.0.103`). The merge left all backend files byte-identical, so backend
  buildability is unchanged from `WorxForge-BE@e7d7061`.

## FE branch migration

All 27 FE branches are migrated into the monorepo namespaced as `legacy-fe/<original-name>`
to avoid collisions with existing BE branch names. All FE commit history is additionally
reachable via the merged `main`.

## Rollback procedure

- Before push: the migration exists only on `chore/merge-fe-monorepo`; delete the branch or the working clone. `main` is untouched.
- After branch push, before PR merge: `git push origin --delete chore/merge-fe-monorepo`.
- After PR merge (worst case): revert the merge commit via a new PR. `main` is never force-pushed.
- Ultimate fallback: the mirror backups above.

## Post-migration actions (pending approval)

- [ ] Push `chore/merge-fe-monorepo` and open the PR (merge-commit strategy — not squash/rebase).
- [ ] Push all 27 FE branches as `legacy-fe/*`.
- [ ] Rename `itworx/WorxForge-BE` → `itworx/WorxForge`.
- [ ] Add an archival notice to `WorxForge-FE` README and archive the repository.
- [ ] (Optional) Add a `.gitattributes` for explicit line-ending normalization (`*.sh eol=lf`, `*.ps1 eol=crlf`).
