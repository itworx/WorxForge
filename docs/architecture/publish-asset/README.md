# Publish Asset — Implementation Documentation

This folder contains the complete implementation documentation for the **Publish Asset** feature.

---

## Overview

The Publish Asset feature allows authenticated users to publish a new asset to the platform.

A published asset consists of:

- A **unique name** that acts as a permanent global identifier
- A **description** in plain text
- **Markdown content** edited in a split editor with live preview
- An **asset type**, **domain**, and **use case** (all from seeded reference data)
- One or more **compatible AI tools**
- Zero or more **tags** that are normalized to lowercase and deduplicated

---

## Architecture

The feature follows the existing **Clean Architecture + CQRS** pattern:

```
API Layer (AssetsEndpointGroup)
    ↓
Core Layer (Handlers → Services → Repositories)
    ↓
Infrastructure Layer (EF Core → SQL Server)
    ↓
Domain Layer (Entities, Commands, Queries, DTOs)
```

No new architectural patterns were introduced.

---

## Phase Documents

| Phase | File | Summary |
|---|---|---|
| 1 | [phase-01-database-migration.md](phase-01-database-migration.md) | EF Core migration, schema changes |
| 2 | [phase-02-domain-layer.md](phase-02-domain-layer.md) | Entities, commands, queries, DTOs |
| 3 | [phase-03-repository-layer.md](phase-03-repository-layer.md) | Repository interfaces and implementations |
| 4 | [phase-04-service-layer.md](phase-04-service-layer.md) | Business logic, tag normalization, FK validation |
| 5 | [phase-05-cqrs.md](phase-05-cqrs.md) | Commands, queries, handlers, validators |
| 6 | [phase-06-api-layer.md](phase-06-api-layer.md) | Endpoints, request models, error mapping |
| 7 | [phase-07-markdown-upload.md](phase-07-markdown-upload.md) | Upload lifecycle, UTF-8 validation |
| 8 | [phase-08-tag-normalization.md](phase-08-tag-normalization.md) | Normalization rules, deduplication, GetOrCreate |
| 9 | [phase-09-publish-flow.md](phase-09-publish-flow.md) | Full end-to-end publish lifecycle |
| 10 | [phase-10-frontend-integration.md](phase-10-frontend-integration.md) | Upload, editor, preview, autosave, error handling |
| 11 | [phase-11-tests.md](phase-11-tests.md) | Unit and integration test coverage |

---

## Key Design Decisions

| Decision | Reason |
|---|---|
| No new packages | Project constraint |
| `UniqueName` replaces `Title` | Assets need a globally unique, permanent identifier |
| CQRS for every endpoint | Enforced by DI convention scanner (`*Handler`, `*Validator`) |
| Upload handler works inline | No DB writes; no service delegation needed |
| Single `SaveChangesAsync` for publish | EF Core wraps it in an implicit transaction |
| Tag normalization in service layer | Business rule, not input validation |
| FK validation in service layer | Requires DB access; not appropriate for validators |
| Race condition handled at DB level | Unique index catches concurrent duplicates |
| `LookupItemDto` reused for all 4 lookups | Already exists; no new DTO types |
| Validator = input rules only | Clean separation; no duplication with service |
</content>
</invoke>