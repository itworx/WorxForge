# Publish Asset — Complete Implementation Documentation

## Table of Contents

1. [Overview](#overview)
2. [File Ownership](#file-ownership)
3. [Architecture & Patterns](#architecture--patterns)
4. [Phase 1 — Database Migration](#phase-1--database-migration)
5. [Phase 2 — Domain Layer](#phase-2--domain-layer)
6. [Phase 3 — Repository Layer](#phase-3--repository-layer)
7. [Phase 4 — Service Layer](#phase-4--service-layer)
8. [Phase 5 — CQRS: Commands, Queries, Handlers, Validators](#phase-5--cqrs-commands-queries-handlers-validators)
9. [Phase 6 — API Layer](#phase-6--api-layer)
10. [Phase 7 — Markdown Upload Flow](#phase-7--markdown-upload-flow)
11. [Phase 8 — Tag Normalization](#phase-8--tag-normalization)
12. [Phase 9 — Full Publish Flow](#phase-9--full-publish-flow)
13. [Phase 10 — Frontend Integration](#phase-10--frontend-integration)
14. [Phase 11 — Tests](#phase-11--tests)
15. [Key Design Decisions](#key-design-decisions)

---

## Overview

The **Publish Asset** feature allows an authenticated user to publish a new asset to the platform.

A published asset consists of:

| Field | Description |
|---|---|
| `UniqueName` | Globally unique permanent identifier (≤ 200 chars) |
| `Description` | Plain text description (≤ 1000 chars) |
| `MarkdownContent` | Full markdown body, edited in a split editor |
| `AssetTypeId` | FK to seeded `ASSET_TYPE` reference table |
| `DomainId` | FK to seeded `DOMAIN` reference table |
| `UseCaseId` | FK to seeded `USE_CASE` reference table |
| `ToolIds` | One or more FKs to seeded `TOOL` reference table |
| `Tags` | Zero or more free-text tags, normalized to lowercase |

### What is explicitly out of scope

- Edit Asset, Delete Asset, Search, Asset Details
- Drafts, Publish Status, Thumbnails, Image Upload, Version History
- Any new NuGet packages

---

## File Ownership

The full Publish Asset feature spans multiple concerns. The table below shows who owns what.

### `POST /api/assets` — Publish endpoint owner

These files are the sole responsibility of the developer implementing the publish endpoint:

| File | Status | Change |
|---|---|---|
| `WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs` | Modified | `CreateAssetAsync` method + route registration |
| `WorxForge.API/Models/Requests/Assets/CreateAssetRequest.cs` | Modified | Was an empty class — populated with fields |
| `WorxForge.API/Middlewares/ExceptionMiddleware.cs` | Modified | Was a bare 500 catch-all — expanded to map typed exceptions |
| `WorxForge.Core/Handlers/Assets/CreateAssetCommandHandler.cs` | Modified | Was a stub — implemented |
| `WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs` | Modified | Was a stub — implemented |
| `WorxForge.Core/Interfaces/Repositories/IAssetRepository.cs` | Modified | Added `ExistsByUniqueNameAsync` + `CreateAsync` |
| `WorxForge.Infrastructure/Repositories/AssetRepository.cs` | Modified | Implemented those 2 new methods |
| `WorxForge.Infrastructure/Services/AssetService.cs` | Modified | `CreateAsync` implemented (was a stub) |
| `WorxForge.Domain/Commands/Assets/CreateAssetCommand.cs` | Modified | `Title` → `UniqueName`, `Version` removed |
| `WorxForge.Domain/Dtos/Assets/PublishAssetResponse.cs` | **New** | — |
| `WorxForge.Domain/Exceptions/ValidationException.cs` | **New** | — |
| `WorxForge.Domain/Exceptions/ConflictException.cs` | **New** | — |
| `WorxForge.Domain/Exceptions/NotFoundException.cs` | **New** | — |

### Schema rename (`Title` → `UniqueName`) — committed separately

These files were committed in a dedicated `refactor` commit and require team awareness since they touch shared domain types:

| File | Change |
|---|---|
| `WorxForge.Domain/Entities/Asset.cs` | `Title` → `UniqueName`, `Description` non-nullable |
| `WorxForge.Domain/Commands/Assets/UpdateAssetCommand.cs` | Same renames |
| `WorxForge.Domain/Dtos/Assets/AssetDetailsDto.cs` | Same renames |
| `WorxForge.Domain/Dtos/Assets/AssetSummaryDto.cs` | Same renames |
| `WorxForge.Domain/Dtos/Assets/MyAssetDto.cs` | Same renames, `Version` removed |
| `WorxForge.Domain/Dtos/Admin/AdminAssetDto.cs` | Same renames |
| `WorxForge.Infrastructure/Configuration/WorxForgeDbContext.cs` | Added `IsRequired` + `HasMaxLength` + `HasIndex(IsUnique)` for `UniqueName` and `Description` |
| `WorxForge.Infrastructure/Migrations/20260806112754_RenameAssetTitleToUniqueName.cs` | EF Core migration |
| `WorxForge.Infrastructure/Migrations/WorxForgeDbContextModelSnapshot.cs` | Auto-updated by EF tooling |
| `WorxForge.Infrastructure/Repositories/AssetRepository.cs` | Updated projection `Title` → `UniqueName` |

### Supporting endpoints — separate concerns

These belong to supporting features of the publish screen but are not the publish action:

| File | Endpoint |
|---|---|
| `WorxForge.Core/Handlers/Lookup/GetAssetTypesQueryHandler.cs` | `GET /api/assets/types` |
| `WorxForge.Core/Handlers/Lookup/GetDomainsQueryHandler.cs` | `GET /api/assets/domains` |
| `WorxForge.Core/Handlers/Lookup/GetUseCasesQueryHandler.cs` | `GET /api/assets/use-cases` |
| `WorxForge.Core/Handlers/Lookup/GetToolsQueryHandler.cs` | `GET /api/assets/tools` |
| `WorxForge.Core/Handlers/Assets/UploadMarkdownFileCommandHandler.cs` | `POST /api/assets/upload-markdown` |
| `WorxForge.Core/Validators/Assets/UploadMarkdownFileCommandValidator.cs` | Upload validation |
| `WorxForge.Domain/Commands/Assets/UploadMarkdownFileCommand.cs` | Upload command |
| `WorxForge.Domain/Dtos/Assets/UploadMarkdownResponse.cs` | Upload response |
| `WorxForge.Domain/Queries/Lookup/` (4 files) | Lookup queries |
| `WorxForge.Infrastructure/Repositories/AssetTypeRepository.cs` | Lookup data |
| `WorxForge.Infrastructure/Repositories/BusinessDomainRepository.cs` | Lookup data |
| `WorxForge.Infrastructure/Repositories/UseCaseRepository.cs` | Lookup data |
| `WorxForge.Infrastructure/Repositories/ToolRepository.cs` | Lookup data |
| `WorxForge.Infrastructure/Repositories/TagRepository.cs` | Tag upsert |

---

## Architecture & Patterns

The feature reuses the existing **Clean Architecture + CQRS** pattern without introducing any new abstractions.

```
HTTP Request
    ↓
AssetsEndpointGroup          (WorxForge.API)
    ↓
ICommandHandler / IQueryHandler  (WorxForge.Core — auto-registered by DI scanner)
    ↓
IRequestValidator            (WorxForge.Core — validates input format only)
    ↓
IAssetService / IXxxRepository   (WorxForge.Core interfaces)
    ↓
AssetService / XxxRepository     (WorxForge.Infrastructure — business logic + EF Core)
    ↓
WorxForgeDbContext               (WorxForge.Infrastructure — SQL Server)
```

**DI registration convention:** The `AddWorxForgeCore()` method scans the Core assembly for classes whose name ends in `Handler` or `Validator` and registers all their interfaces as `Transient`. All new handlers and validators in this feature are discovered automatically — no manual registration needed.

---

## Phase 1 — Database Migration

### Goal

Rename `Asset.Title` → `Asset.UniqueName`, enforce global uniqueness with a database index, and make `Description` NOT NULL.

### Why these changes

**`Title` → `UniqueName`:** The original `Title` column had no uniqueness constraint. The domain decision is that an asset name must be a **permanent, globally unique identifier** — similar to a npm package name or a GitHub repository slug. Renaming the column reflects this semantic change throughout the entire stack.

**Unique Index:** The uniqueness constraint is enforced **at the database level** as the final guard. The application-level check (`ExistsByUniqueNameAsync`) handles the common path, but two concurrent publish requests can both pass the application check before either commits. The unique index `IX_ASSET_unique_name` catches this race condition.

**`Description` NOT NULL:** The publish screen requires a description. Making it NOT NULL at the database level enforces the invariant regardless of insertion path.

### DbContext configuration

```csharp
// WorxForge.Infrastructure/Configuration/WorxForgeDbContext.cs

private static void ConfigureAsset(EntityTypeBuilder<Asset> entity)
{
    entity.ToTable("ASSET");
    entity.HasKey(e => e.AssetId);
    entity.Property(e => e.UniqueName).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
    entity.HasIndex(e => e.UniqueName).IsUnique();
    entity.HasOne<User>()
        .WithMany()
        .HasForeignKey(e => e.AuthorId)
        .OnDelete(DeleteBehavior.NoAction);
    entity.HasOne<AssetType>()
        .WithMany()
        .HasForeignKey(e => e.AssetTypeId)
        .OnDelete(DeleteBehavior.NoAction);
    entity.HasOne<BusinessDomain>()
        .WithMany()
        .HasForeignKey(e => e.DomainId)
        .OnDelete(DeleteBehavior.NoAction);
    entity.HasOne<UseCase>()
        .WithMany()
        .HasForeignKey(e => e.UseCaseId)
        .OnDelete(DeleteBehavior.NoAction);
    ApplySnakeCaseColumnNames(entity);
}
```

`ApplySnakeCaseColumnNames` converts `UniqueName` → `unique_name` automatically. All column names in this project use `snake_case`.

### Migration file

`20260806112754_RenameAssetTitleToUniqueName.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "title",
        table: "ASSET");

    migrationBuilder.AlterColumn<string>(
        name: "description",
        table: "ASSET",
        type: "nvarchar(1000)",
        maxLength: 1000,
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "nvarchar(max)",
        oldNullable: true);

    migrationBuilder.AddColumn<string>(
        name: "unique_name",
        table: "ASSET",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: false,
        defaultValue: "");

    migrationBuilder.CreateIndex(
        name: "IX_ASSET_unique_name",
        table: "ASSET",
        column: "unique_name",
        unique: true);
}
```

### Column constraints summary

| Column | SQL Type | Nullable | Constraint |
|---|---|---|---|
| `unique_name` | nvarchar(200) | NOT NULL | UNIQUE INDEX `IX_ASSET_unique_name` |
| `description` | nvarchar(1000) | NOT NULL | — |
| `markdown_content` | nvarchar(max) | NOT NULL | — |

### Applying / rolling back

```bash
# Apply
cd src/WorxForge.Infrastructure
dotnet ef database update --startup-project ../WorxForge.API

# Rollback to previous migration
dotnet ef database update 20260804224229_ChangeUserGoogleIdToString --startup-project ../WorxForge.API
```

---

## Phase 2 — Domain Layer

### Entity: `Asset.cs`

```csharp
// WorxForge.Domain/Entities/Asset.cs

namespace WorxForge.Domain.Entities;

public sealed class Asset
{
    public Guid AssetId { get; set; }
    public Guid AuthorId { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MarkdownContent { get; set; } = string.Empty;
    public Guid AssetTypeId { get; set; }
    public Guid DomainId { get; set; }
    public Guid UseCaseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Changes from before:**
- `Title` → `UniqueName`
- `Description?` (nullable) → `Description` (required, non-nullable)

### Command: `CreateAssetCommand.cs`

```csharp
// WorxForge.Domain/Commands/Assets/CreateAssetCommand.cs

namespace WorxForge.Domain.Commands.Assets;

public sealed class CreateAssetCommand
{
    public Guid AuthorId { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid AssetTypeId { get; init; }
    public Guid DomainId { get; init; }
    public Guid UseCaseId { get; init; }
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string MarkdownContent { get; init; } = string.Empty;
}
```

**Changes from before:** `Title` → `UniqueName`, `Description?` → `Description`, `Version` removed.

`AuthorId` is NOT part of the HTTP request body — it is extracted from the JWT `sub` claim inside the endpoint delegate and injected into the command before dispatching.

`Tags` is `IReadOnlyList<string>`: raw user input. The service layer normalizes them to lowercase and deduplicates before persistence.

### Command: `UploadMarkdownFileCommand.cs`

```csharp
// WorxForge.Domain/Commands/Assets/UploadMarkdownFileCommand.cs

namespace WorxForge.Domain.Commands.Assets;

public sealed class UploadMarkdownFileCommand
{
    public byte[] FileContent { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
}
```

**Why `byte[]` instead of `IFormFile`?** Commands must be pure data objects with no HTTP framework dependencies. The endpoint reads `IFormFile` into a `byte[]` before creating the command. This keeps the domain and core layers entirely free of ASP.NET references.

### Lookup queries

Four parameterless queries, each returning `IReadOnlyList<LookupItemDto>`:

```csharp
// WorxForge.Domain/Queries/Lookup/

public sealed class GetAssetTypesQuery;
public sealed class GetDomainsQuery;
public sealed class GetUseCasesQuery;
public sealed class GetToolsQuery;
```

These are parameterless because all reference data is returned in full — the lists are small (seeded, < 20 items each) and never paginated.

### Response DTOs

```csharp
// WorxForge.Domain/Dtos/Assets/PublishAssetResponse.cs
public sealed class PublishAssetResponse
{
    public Guid AssetId { get; init; }
}

// WorxForge.Domain/Dtos/Assets/UploadMarkdownResponse.cs
public sealed class UploadMarkdownResponse
{
    public string MarkdownContent { get; init; } = string.Empty;
}
```

`PublishAssetResponse` is used by the endpoint to construct the `Location` header (`/api/assets/{AssetId}`) and return a `201 Created` response.

`UploadMarkdownResponse` carries the decoded UTF-8 text back to the frontend, which uses it to populate the markdown editor.

### Updated read DTOs

| DTO | Changes |
|---|---|
| `AssetDetailsDto` | `Title` → `UniqueName`; `Description?` → `Description`; `Version` removed |
| `AssetSummaryDto` | `Title` → `UniqueName`; `Description?` → `Description` |
| `MyAssetDto` | `Title` → `UniqueName`; `Version` removed |
| `AdminAssetDto` | `Title` → `UniqueName` |

### Reused DTO

`LookupItemDto { Guid Id; string Name; }` — already existed in `WorxForge.Domain.Dtos.Explore`. Reused for all four lookup endpoint responses. No new DTO types were created.

### New exception types

```csharp
// WorxForge.Domain/Exceptions/

public sealed class ValidationException(string message) : WorxForgeException(message);
public sealed class ConflictException(string message) : WorxForgeException(message);
public sealed class NotFoundException(string message) : WorxForgeException(message);
```

All three inherit from the existing `WorxForgeException`. They are pure domain types — no ASP.NET references. `ExceptionMiddleware` in the API layer maps them to HTTP status codes.

| Exception | HTTP Status | When thrown |
|---|---|---|
| `ValidationException` | 400 Bad Request | Invalid input format, content, or file |
| `ConflictException` | 409 Conflict | `UniqueName` already taken |
| `NotFoundException` | 422 Unprocessable Entity | Referenced FK (AssetType, Domain, UseCase, Tool) does not exist |

**Why 422 for not-found references instead of 404?** A 404 means the endpoint URL was not found. When a client submits a valid URL but the request body references a `DomainId` that does not exist in the database, the *request* is semantically incorrect — the correct status is 422 Unprocessable Entity.

---

## Phase 3 — Repository Layer

### Interface: `IAssetRepository`

Two new methods were added:

```csharp
// WorxForge.Core/Interfaces/Repositories/IAssetRepository.cs

Task<bool> ExistsByUniqueNameAsync(string uniqueName, CancellationToken cancellationToken);

Task<Guid> CreateAsync(
    Asset asset,
    IEnumerable<Guid> toolIds,
    IEnumerable<Guid> tagIds,
    CancellationToken cancellationToken);
```

**`ExistsByUniqueNameAsync`:** Used by the service layer as a fast-fail check before attempting the insert. Returns `true` if a record with the same `UniqueName` already exists.

**`CreateAsync`:** Accepts the `Asset` entity plus the resolved `toolIds` and `tagIds`. It saves the `Asset` row, all `AssetTool` junction rows, and all `AssetTag` junction rows in a **single `SaveChangesAsync` call**, which EF Core wraps in an implicit transaction. This is the only repository method responsible for persisting a new asset — it is the atomicity boundary.

### Implementation: `AssetRepository`

```csharp
// WorxForge.Infrastructure/Repositories/AssetRepository.cs

public Task<bool> ExistsByUniqueNameAsync(string uniqueName, CancellationToken cancellationToken) =>
    dbContext.Assets.AnyAsync(a => a.UniqueName == uniqueName, cancellationToken);

public async Task<Guid> CreateAsync(
    Asset asset,
    IEnumerable<Guid> toolIds,
    IEnumerable<Guid> tagIds,
    CancellationToken cancellationToken)
{
    dbContext.Assets.Add(asset);
    dbContext.AssetTools.AddRange(
        toolIds.Select(id => new AssetTool { AssetId = asset.AssetId, ToolId = id }));
    dbContext.AssetTags.AddRange(
        tagIds.Select(id => new AssetTag { AssetId = asset.AssetId, TagId = id }));
    await dbContext.SaveChangesAsync(cancellationToken);
    return asset.AssetId;
}
```

**Why a single `SaveChangesAsync` instead of an explicit transaction?**  
EF Core wraps a single `SaveChangesAsync` call in an implicit database transaction. All three `Add`/`AddRange` operations are tracked in the same `DbContext` change tracker and flushed atomically. If any insert fails (e.g., a FK constraint or the unique index), the entire batch is rolled back automatically. An explicit `BeginTransactionAsync` / `CommitAsync` pair is only necessary when you need to span multiple `SaveChangesAsync` calls or include raw SQL — neither applies here.

### Interface: `ITagRepository`

```csharp
// WorxForge.Core/Interfaces/Repositories/ITagRepository.cs

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken);
}
```

`GetOrCreateAsync` is the tag upsert primitive. It receives a **pre-normalized** (trimmed, lowercased) tag name. If the tag already exists, it returns the existing entity. If not, it inserts a new `Tag` row and returns it.

### Implementation: `TagRepository`

```csharp
// WorxForge.Infrastructure/Repositories/TagRepository.cs

public sealed class TagRepository(WorxForgeDbContext dbContext) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Tags.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        if (existing is not null)
            return existing;

        var tag = new Tag { TagId = Guid.NewGuid(), Name = name };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tag;
    }
}
```

**Tag `SaveChangesAsync` vs asset `SaveChangesAsync`:** Tag inserts happen before the asset insert. Each tag is committed independently. If two concurrent requests try to create the same tag simultaneously, the second one will throw a `DbUpdateException` due to the existing unique index on `TAG.name`. This is an acceptable edge case — in practice, tags accumulate gradually and collisions are rare. A production hardening could add a retry or catch-and-re-read pattern.

### Lookup repositories

All four were updated from stubs to full implementations:

```csharp
// AssetTypeRepository
public async Task<IReadOnlyList<AssetType>> GetAllAsync(CancellationToken cancellationToken) =>
    await dbContext.AssetTypes.OrderBy(x => x.Name).ToListAsync(cancellationToken);

public Task<AssetType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
    dbContext.AssetTypes.FirstOrDefaultAsync(x => x.AssetTypeId == id, cancellationToken);

// BusinessDomainRepository
public async Task<IReadOnlyList<BusinessDomain>> GetAllAsync(CancellationToken cancellationToken) =>
    await dbContext.BusinessDomains.OrderBy(x => x.Name).ToListAsync(cancellationToken);

public Task<BusinessDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
    dbContext.BusinessDomains.FirstOrDefaultAsync(x => x.DomainId == id, cancellationToken);

// UseCaseRepository
public async Task<IReadOnlyList<UseCase>> GetAllAsync(CancellationToken cancellationToken) =>
    await dbContext.UseCases.OrderBy(x => x.Name).ToListAsync(cancellationToken);

public Task<UseCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
    dbContext.UseCases.FirstOrDefaultAsync(x => x.UseCaseId == id, cancellationToken);

// ToolRepository
public async Task<IReadOnlyList<Tool>> GetAllAsync(CancellationToken cancellationToken) =>
    await dbContext.Tools.OrderBy(x => x.Name).ToListAsync(cancellationToken);

public Task<Tool?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
    dbContext.Tools.FirstOrDefaultAsync(x => x.ToolId == id, cancellationToken);
```

`GetByIdAsync` is used exclusively by `AssetService.CreateAsync` to validate FK references. It returns `null` if the ID is not found, which the service converts to a `NotFoundException`.

All lists are ordered alphabetically by `Name` to ensure consistent dropdown ordering across environments.

---

## Phase 4 — Service Layer

### `AssetService.CreateAsync` — full implementation

This is the core business logic method. It is responsible for all **business rules** — not input format validation (that belongs to the validator).

```csharp
// WorxForge.Infrastructure/Services/AssetService.cs

public async Task<Guid> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken)
{
    // 1. Fast-fail uniqueness check (application level).
    //    The DB unique index provides the definitive guard against race conditions.
    if (await assetRepository.ExistsByUniqueNameAsync(command.UniqueName, cancellationToken))
        throw new ConflictException($"An asset with the name '{command.UniqueName}' already exists.");

    // 2. FK validation — reject early if any referenced entity does not exist.
    if (await assetTypeRepository.GetByIdAsync(command.AssetTypeId, cancellationToken) is null)
        throw new NotFoundException($"Asset type '{command.AssetTypeId}' was not found.");

    if (await businessDomainRepository.GetByIdAsync(command.DomainId, cancellationToken) is null)
        throw new NotFoundException($"Domain '{command.DomainId}' was not found.");

    if (await useCaseRepository.GetByIdAsync(command.UseCaseId, cancellationToken) is null)
        throw new NotFoundException($"Use case '{command.UseCaseId}' was not found.");

    // 3. Deduplicate tool IDs defensively before FK validation and before saving.
    //    The validator already rejects duplicates, but this is a second safety net.
    var toolIds = command.ToolIds.Distinct().ToList();

    foreach (var toolId in toolIds)
    {
        if (await toolRepository.GetByIdAsync(toolId, cancellationToken) is null)
            throw new NotFoundException($"Tool '{toolId}' was not found.");
    }

    // 4. Normalize and deduplicate tags.
    var normalizedTags = command.Tags
        .Select(t => t.Trim().ToLowerInvariant())
        .Where(t => t.Length > 0)
        .Distinct(StringComparer.Ordinal)
        .ToList();

    // 5. Resolve each tag to a TagId — reuse existing or create new.
    var tagIds = new List<Guid>(normalizedTags.Count);
    foreach (var tagName in normalizedTags)
    {
        var tag = await tagRepository.GetOrCreateAsync(tagName, cancellationToken);
        tagIds.Add(tag.TagId);
    }

    // 6. Build entity.
    var asset = new Asset
    {
        AssetId    = Guid.NewGuid(),
        AuthorId   = command.AuthorId,
        UniqueName = command.UniqueName,
        Description   = command.Description,
        MarkdownContent = command.MarkdownContent,
        AssetTypeId = command.AssetTypeId,
        DomainId    = command.DomainId,
        UseCaseId   = command.UseCaseId,
        CreatedAt   = DateTime.UtcNow,
        UpdatedAt   = DateTime.UtcNow
    };

    // 7. Persist atomically. Catch unique constraint violation for race condition.
    try
    {
        return await assetRepository.CreateAsync(asset, toolIds, tagIds, cancellationToken);
    }
    catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
    {
        // Race condition: another request committed the same UniqueName between our check and our insert.
        throw new ConflictException($"An asset with the name '{command.UniqueName}' already exists.");
    }
}

// SQL Server error 2601 = unique index violation; 2627 = unique constraint violation.
private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
    ex.InnerException?.Message.Contains("unique_name", StringComparison.OrdinalIgnoreCase) == true ||
    ex.InnerException?.Message.Contains("IX_ASSET_unique_name", StringComparison.OrdinalIgnoreCase) == true;
```

### Validation boundary

| Responsibility | Layer |
|---|---|
| Required fields, length limits, format checks, duplicate ToolIds, tag count | **Validator** (`CreateAssetCommandValidator`) |
| UniqueName uniqueness, FK existence, tag normalization, tool dedup | **Service** (`AssetService`) |

These responsibilities never overlap. The validator runs first (in the handler, before the service call). If validation passes, the service applies business rules. This separation ensures each layer is testable independently and has a single, clear responsibility.

### Constructor injection

```csharp
public sealed class AssetService(
    IAssetRepository assetRepository,
    IAssetTypeRepository assetTypeRepository,
    IBusinessDomainRepository businessDomainRepository,
    IUseCaseRepository useCaseRepository,
    IToolRepository toolRepository,
    ITagRepository tagRepository) : IAssetService
```

All six dependencies are registered as `Scoped` in `AddWorxForgeInfrastructure`. The service itself is also `Scoped`, so all repositories share the same `DbContext` instance within a single HTTP request.

---

## Phase 5 — CQRS: Commands, Queries, Handlers, Validators

The project enforces CQRS for every endpoint via the DI convention scanner. Every endpoint injects `ICommandHandler<TCommand, TResponse>` or `IQueryHandler<TQuery, TResponse>` directly as a parameter. There is no way to bypass this pattern.

### Validators

#### `CreateAssetCommandValidator`

```csharp
// WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs

public sealed class CreateAssetCommandValidator : IRequestValidator<CreateAssetCommand>
{
    private const int UniqueNameMaxLength = 200;
    private const int DescriptionMaxLength = 1000;
    private const int TagMaxLength = 50;
    private const int TagsMaxCount = 20;

    public Task ValidateAsync(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AuthorId == Guid.Empty)
            throw new ValidationException("Author is required.");

        if (string.IsNullOrWhiteSpace(request.UniqueName))
            throw new ValidationException("Unique name is required.");

        if (request.UniqueName.Length > UniqueNameMaxLength)
            throw new ValidationException($"Unique name must not exceed {UniqueNameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ValidationException("Description is required.");

        if (request.Description.Length > DescriptionMaxLength)
            throw new ValidationException($"Description must not exceed {DescriptionMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.MarkdownContent))
            throw new ValidationException("Markdown content is required.");

        if (request.AssetTypeId == Guid.Empty)
            throw new ValidationException("Asset type is required.");

        if (request.DomainId == Guid.Empty)
            throw new ValidationException("Domain is required.");

        if (request.UseCaseId == Guid.Empty)
            throw new ValidationException("Use case is required.");

        if (request.ToolIds.Count == 0)
            throw new ValidationException("At least one compatible AI tool is required.");

        if (request.ToolIds.Count != request.ToolIds.Distinct().Count())
            throw new ValidationException("Duplicate tool IDs are not allowed.");

        if (request.Tags.Count > TagsMaxCount)
            throw new ValidationException($"A maximum of {TagsMaxCount} tags is allowed.");

        foreach (var tag in request.Tags)
        {
            if (tag.Trim().Length > TagMaxLength)
                throw new ValidationException($"Each tag must not exceed {TagMaxLength} characters.");
        }

        return Task.CompletedTask;
    }
}
```

**Rules enforced:**

| Rule | Why here (not service) |
|---|---|
| `AuthorId` not empty | Input format — the claim value must parse to a valid Guid |
| `UniqueName` required, ≤ 200 chars | Input format check |
| `Description` required, ≤ 1000 chars | Input format check |
| `MarkdownContent` not empty | Input format check |
| FK Guids not empty | Input format — empty Guid means the field was not sent |
| `ToolIds` ≥ 1 | Input format — at least one selection required |
| `ToolIds` no duplicates | Input format — the client should never send the same ID twice |
| `Tags.Count` ≤ 20 | Input format — volume limit before any DB interaction |
| Each tag ≤ 50 chars (before normalization) | Input format — raw length check |

**Not enforced here:** UniqueName uniqueness (requires a DB call), FK existence (requires DB calls), tag normalization (business logic).

#### `UploadMarkdownFileCommandValidator`

```csharp
// WorxForge.Core/Validators/Assets/UploadMarkdownFileCommandValidator.cs

public sealed class UploadMarkdownFileCommandValidator : IRequestValidator<UploadMarkdownFileCommand>
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public Task ValidateAsync(UploadMarkdownFileCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.FileContent.Length == 0)
            throw new ValidationException("The uploaded file is empty.");

        if (request.FileContent.Length > MaxFileSizeBytes)
            throw new ValidationException("The uploaded file exceeds the 5 MB limit.");

        if (!Path.GetExtension(request.FileName).Equals(".md", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Only .md files are allowed.");

        return Task.CompletedTask;
    }
}
```

**Why 5 MB limit?** An OWASP best practice for file upload endpoints. A valid `.md` file containing markdown text will almost never exceed 1 MB — 5 MB provides a generous buffer while preventing abuse.

**Why `StringComparison.OrdinalIgnoreCase` for extension?** Accepts `.md`, `.MD`, `.Md` etc. A user on macOS or Windows may save a file with any casing. Extension validation must be case-insensitive.

### Handlers

#### `CreateAssetCommandHandler`

```csharp
// WorxForge.Core/Handlers/Assets/CreateAssetCommandHandler.cs

public sealed class CreateAssetCommandHandler(
    IRequestValidator<CreateAssetCommand> validator,
    IAssetService assetService) : ICommandHandler<CreateAssetCommand, PublishAssetResponse>
{
    public async Task<PublishAssetResponse> HandleAsync(
        CreateAssetCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        var assetId = await assetService.CreateAsync(command, cancellationToken);
        return new PublishAssetResponse { AssetId = assetId };
    }
}
```

The handler is intentionally thin. It has exactly three responsibilities: validate, delegate to the service, and wrap the result. No business logic lives here.

#### `UploadMarkdownFileCommandHandler`

```csharp
// WorxForge.Core/Handlers/Assets/UploadMarkdownFileCommandHandler.cs

public sealed class UploadMarkdownFileCommandHandler(
    IRequestValidator<UploadMarkdownFileCommand> validator)
    : ICommandHandler<UploadMarkdownFileCommand, UploadMarkdownResponse>
{
    public async Task<UploadMarkdownResponse> HandleAsync(
        UploadMarkdownFileCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);

        string markdownContent;
        try
        {
            // Throw on invalid bytes instead of silently replacing them with the replacement character.
            var utf8 = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);
            markdownContent = utf8.GetString(command.FileContent).Trim();
        }
        catch (DecoderFallbackException)
        {
            throw new ValidationException("The uploaded file is not valid UTF-8.");
        }

        if (string.IsNullOrEmpty(markdownContent))
            throw new ValidationException("The uploaded file contains no markdown content.");

        return new UploadMarkdownResponse { MarkdownContent = markdownContent };
    }
}
```

**Why `new UTF8Encoding(throwOnInvalidBytes: true)` instead of `Encoding.UTF8.GetString()`?**

`Encoding.UTF8` uses a `DecoderReplacementFallback` — it silently replaces invalid byte sequences with the Unicode replacement character (`\uFFFD`, displayed as `�`). This means a corrupted binary file could be "decoded" into garbage text and stored in the database. Using `throwOnInvalidBytes: true` rejects any file that is not valid UTF-8 with a `400 Bad Request` before anything is stored.

**Why no `IAssetService` injection?** This operation has no database interaction — it is a pure validation + text extraction. Introducing a service method for this would add an abstraction with no value.

#### Lookup handlers

All four follow the same pattern: call `GetAllAsync` on the repository, project to `LookupItemDto` using inline LINQ:

```csharp
// GetAssetTypesQueryHandler
public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(
    GetAssetTypesQuery query, CancellationToken cancellationToken)
{
    var types = await assetTypeRepository.GetAllAsync(cancellationToken);
    return types.Select(x => new LookupItemDto { Id = x.AssetTypeId, Name = x.Name }).ToList();
}

// GetDomainsQueryHandler
public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(
    GetDomainsQuery query, CancellationToken cancellationToken)
{
    var domains = await domainRepository.GetAllAsync(cancellationToken);
    return domains.Select(x => new LookupItemDto { Id = x.DomainId, Name = x.Name }).ToList();
}

// GetUseCasesQueryHandler
public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(
    GetUseCasesQuery query, CancellationToken cancellationToken)
{
    var useCases = await useCaseRepository.GetAllAsync(cancellationToken);
    return useCases.Select(x => new LookupItemDto { Id = x.UseCaseId, Name = x.Name }).ToList();
}

// GetToolsQueryHandler
public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(
    GetToolsQuery query, CancellationToken cancellationToken)
{
    var tools = await toolRepository.GetAllAsync(cancellationToken);
    return tools.Select(x => new LookupItemDto { Id = x.ToolId, Name = x.Name }).ToList();
}
```

**Why inline LINQ instead of AutoMapper?** The existing `DomainMappingProfile` is a stub that does not extend `Profile`. All existing projections in the codebase (e.g., `AssetRepository.SelectAssetSummaries`) use inline LINQ `.Select()`. This is consistent with the established pattern.

---

## Phase 6 — API Layer

### Exception middleware

```csharp
// WorxForge.API/Middlewares/ExceptionMiddleware.cs

public sealed class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (WorxForgeException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            throw;
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
```

The middleware was updated from a bare 500 catch-all to a full typed exception mapper. The catch order matters — more specific subtypes (`ValidationException`, `ConflictException`, `NotFoundException`) are caught before the base `WorxForgeException`. Unrecognized exceptions re-throw (preserving the stack trace) and result in a 500.

### Request model

```csharp
// WorxForge.API/Models/Requests/Assets/CreateAssetRequest.cs

public sealed class CreateAssetRequest
{
    public string UniqueName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MarkdownContent { get; init; } = string.Empty;
    public Guid AssetTypeId { get; init; }
    public Guid DomainId { get; init; }
    public Guid UseCaseId { get; init; }
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}
```

`AuthorId` is intentionally absent — it is extracted from the JWT `sub` claim inside the endpoint and injected into `CreateAssetCommand` before dispatch. This prevents a client from impersonating another user by sending a different `AuthorId` in the body.

### Endpoints: `AssetsEndpointGroup`

```csharp
// WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs

public static RouteGroupBuilder MapAssetsEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/assets");

    // Reference data — no auth required, public
    group.MapGet("/types",      GetAssetTypesAsync);
    group.MapGet("/domains",    GetDomainsAsync);
    group.MapGet("/use-cases",  GetUseCasesAsync);
    group.MapGet("/tools",      GetToolsAsync);

    // File upload — auth required, antiforgery disabled (multipart)
    group.MapPost("/upload-markdown", UploadMarkdownAsync)
        .RequireAuthorization(PolicyConstants.AuthenticatedUser)
        .DisableAntiforgery();

    // Publish — auth required
    group.MapPost("/", CreateAssetAsync)
        .RequireAuthorization(PolicyConstants.AuthenticatedUser);

    // ... remaining stubs unchanged
    return group;
}
```

**Why `.DisableAntiforgery()` on the upload endpoint?** ASP.NET minimal APIs enable antiforgery protection for `multipart/form-data` POST requests by default. Since this API is consumed by a JavaScript SPA using Bearer tokens (not cookies + form posts), antiforgery tokens do not apply and must be disabled to allow the multipart upload.

#### Lookup endpoints (identical pattern × 4)

```csharp
private static async Task<IResult> GetAssetTypesAsync(
    IQueryHandler<GetAssetTypesQuery, IReadOnlyList<LookupItemDto>> handler,
    CancellationToken cancellationToken)
{
    var result = await handler.HandleAsync(new GetAssetTypesQuery(), cancellationToken);
    return Results.Ok(result);
}
```

The handler is injected directly as a parameter by the minimal API DI system. ASP.NET resolves `IQueryHandler<GetAssetTypesQuery, IReadOnlyList<LookupItemDto>>` → `GetAssetTypesQueryHandler` automatically from the container.

#### Upload endpoint

```csharp
private static async Task<IResult> UploadMarkdownAsync(
    IFormFile file,
    ICommandHandler<UploadMarkdownFileCommand, UploadMarkdownResponse> handler,
    CancellationToken cancellationToken)
{
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream, cancellationToken);
    var bytes = memoryStream.ToArray();

    var result = await handler.HandleAsync(
        new UploadMarkdownFileCommand { FileContent = bytes, FileName = file.FileName },
        cancellationToken);

    return Results.Ok(result);
}
```

The endpoint's only job is: read the `IFormFile` stream into a `byte[]`, create the command, dispatch it, and return the result. All validation and decoding happen inside the handler.

#### Publish endpoint

```csharp
private static async Task<IResult> CreateAssetAsync(
    [FromBody] CreateAssetRequest request,
    HttpContext httpContext,
    ICommandHandler<CreateAssetCommand, PublishAssetResponse> handler,
    CancellationToken cancellationToken)
{
    var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;
    if (!Guid.TryParse(userIdClaim, out var authorId))
        throw new WorxForgeException("Authenticated user identifier is invalid.");

    var command = new CreateAssetCommand
    {
        AuthorId        = authorId,
        UniqueName      = request.UniqueName,
        Description     = request.Description,
        MarkdownContent = request.MarkdownContent,
        AssetTypeId     = request.AssetTypeId,
        DomainId        = request.DomainId,
        UseCaseId       = request.UseCaseId,
        ToolIds         = request.ToolIds,
        Tags            = request.Tags
    };

    var response = await handler.HandleAsync(command, cancellationToken);
    return Results.Created($"/api/assets/{response.AssetId}", response);
}
```

`ClaimConstants.UserId = "sub"` — the JWT subject claim, set by the authentication infrastructure during login.

`Results.Created(...)` returns HTTP 201 with a `Location` header pointing to the new asset's detail URL.

### Endpoint reference table

| Route | Method | Auth | Request | Success | Error codes |
|---|---|---|---|---|---|
| `/api/assets/types` | GET | None | — | 200 + `LookupItemDto[]` | — |
| `/api/assets/domains` | GET | None | — | 200 + `LookupItemDto[]` | — |
| `/api/assets/use-cases` | GET | None | — | 200 + `LookupItemDto[]` | — |
| `/api/assets/tools` | GET | None | — | 200 + `LookupItemDto[]` | — |
| `/api/assets/upload-markdown` | POST | Required | `multipart/form-data` | 200 + `UploadMarkdownResponse` | 400 |
| `/api/assets` | POST | Required | JSON `CreateAssetRequest` | 201 + `PublishAssetResponse` + `Location` | 400, 401, 409, 422 |

---

## Phase 7 — Markdown Upload Flow

The upload endpoint exists purely to let the frontend populate the markdown editor from a file. The file is **never stored** — only its decoded text content is returned.

### Step-by-step

```
1.  Client selects a file using a file picker filtered to .md
2.  Client sends: POST /api/assets/upload-markdown
                  Authorization: Bearer <token>
                  Content-Type: multipart/form-data
                  Body: file=<binary content>

3.  Endpoint reads IFormFile into byte[]:
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

4.  Endpoint creates command:
        new UploadMarkdownFileCommand { FileContent = bytes, FileName = file.FileName }

5.  Handler calls validator:
    → FileContent.Length == 0            → 400 "The uploaded file is empty."
    → FileContent.Length > 5_242_880     → 400 "The uploaded file exceeds the 5 MB limit."
    → Extension != ".md" (case-insensitive) → 400 "Only .md files are allowed."

6.  Handler decodes bytes:
        var utf8 = new UTF8Encoding(false, throwOnInvalidBytes: true);
        markdownContent = utf8.GetString(bytes).Trim();
    → DecoderFallbackException            → 400 "The uploaded file is not valid UTF-8."
    → Empty after trim                    → 400 "The uploaded file contains no markdown content."

7.  Handler returns:
        new UploadMarkdownResponse { MarkdownContent = markdownContent }

8.  Endpoint returns:
        200 OK
        { "markdownContent": "# Hello World\n\n..." }

9.  Frontend receives the response and sets the markdown editor content.
```

### What the backend does NOT do

- Does not store the file anywhere (disk, DB, blob storage)
- Does not render the markdown to HTML (rendering is frontend-only)
- Does not validate markdown syntax (any valid UTF-8 text is accepted)
- Does not strip or sanitize markdown (the text is stored and rendered as-is)

---

## Phase 8 — Tag Normalization

Tag normalization runs inside `AssetService.CreateAsync`, after all FK validations pass.

### Rules

| Input tag | After normalization |
|---|---|
| `"React"` | `"react"` |
| `"REACT"` | `"react"` |
| `"react"` | `"react"` |
| `" React "` | `"react"` |
| `"TypeScript"` | `"typescript"` |
| `""` | *(filtered out)* |
| `"   "` | *(filtered out)* |

All three of `"React"`, `"REACT"`, `"react"` submitted in the same publish request → stored as a single tag `"react"`.

### Implementation

```csharp
var normalizedTags = command.Tags
    .Select(t => t.Trim().ToLowerInvariant())  // step 1: trim whitespace + lowercase
    .Where(t => t.Length > 0)                  // step 2: drop blanks
    .Distinct(StringComparer.Ordinal)          // step 3: deduplicate (after normalization, case variants are identical)
    .ToList();
```

### `GetOrCreate` flow

For each unique normalized tag name, `TagRepository.GetOrCreateAsync` is called:

```
"react" → SELECT FROM TAG WHERE name = 'react'
         → found?  → return existing Tag
         → not found? → INSERT INTO TAG VALUES (newGuid, 'react')
                       → return new Tag
```

The `TagId` is collected and passed to `AssetRepository.CreateAsync`, which inserts one `ASSET_TAG` row per tag.

### Why not `NormalizedName`?

The plan explicitly forbids introducing a `NormalizedName` column. Because all tags are stored already normalized (always lowercase), the `Tag.Name` column itself serves as the normalized form. Lookups are always by exact lowercase string — no additional column is needed.

### Database constraint as the deduplication backstop

`TAG.name` has a unique index. If two concurrent publish requests both try to create the same tag simultaneously, the second insert will throw a `DbUpdateException`. This is acceptable because the `FirstOrDefaultAsync` read happens before the insert — the race window is extremely narrow and primarily theoretical for tags.

---

## Phase 9 — Full Publish Flow

End-to-end sequence from the moment the user clicks "Publish" to the moment the success response is returned.

```
1.  User clicks "Publish" on the frontend.

2.  Frontend sends:
        POST /api/assets
        Authorization: Bearer <jwt>
        Content-Type: application/json
        {
            "uniqueName": "my-clean-arch-prompt",
            "description": "A prompt for Clean Architecture...",
            "markdownContent": "# Clean Architecture Prompt\n...",
            "assetTypeId": "11111111-...",
            "domainId": "21111111-...",
            "useCaseId": "31111111-...",
            "toolIds": ["41111111-..."],
            "tags": ["Clean Architecture", "SOLID", "clean architecture"]
        }

3.  AssetsEndpointGroup.CreateAssetAsync:
    a. Extracts authorId from JWT sub claim.
    b. Builds CreateAssetCommand from request + authorId.
    c. Calls ICommandHandler<CreateAssetCommand, PublishAssetResponse>.HandleAsync(command, ct).

4.  CreateAssetCommandHandler.HandleAsync:
    a. Calls CreateAssetCommandValidator.ValidateAsync(command, ct).
       → Validates: authorId not empty, uniqueName required ≤200,
                    description required ≤1000, markdownContent not empty,
                    assetTypeId/domainId/useCaseId not empty,
                    toolIds ≥1 and no duplicates,
                    tags ≤20 and each tag ≤50 chars
       → If any rule fails: throws ValidationException → ExceptionMiddleware → 400
    b. Calls IAssetService.CreateAsync(command, ct).

5.  AssetService.CreateAsync:
    a. ExistsByUniqueNameAsync("my-clean-arch-prompt")
       → returns true? throw ConflictException → 409
       → returns false? continue

    b. GetByIdAsync(assetTypeId) → not null? continue → null? throw NotFoundException → 422
    c. GetByIdAsync(domainId)    → not null? continue → null? throw NotFoundException → 422
    d. GetByIdAsync(useCaseId)   → not null? continue → null? throw NotFoundException → 422

    e. toolIds = ["41111111-..."].Distinct().ToList()   (dedup, already 1 item)
    f. GetByIdAsync(toolId) for each → not null? continue → null? throw NotFoundException → 422

    g. Normalize tags:
       "Clean Architecture" → "clean architecture"
       "SOLID"              → "solid"
       "clean architecture" → "clean architecture"  (duplicate of step 1 after normalization)
       After Distinct: ["clean architecture", "solid"]

    h. TagRepository.GetOrCreateAsync("clean architecture") → Tag { TagId: X }
       TagRepository.GetOrCreateAsync("solid")             → Tag { TagId: Y }
       tagIds = [X, Y]

    i. Build Asset entity:
       { AssetId: newGuid, AuthorId: <from JWT>, UniqueName: "my-clean-arch-prompt",
         Description: "...", MarkdownContent: "...", AssetTypeId: ...,
         DomainId: ..., UseCaseId: ..., CreatedAt: UtcNow, UpdatedAt: UtcNow }

    j. AssetRepository.CreateAsync(asset, [toolId], [X, Y], ct):
       → context.Assets.Add(asset)
       → context.AssetTools.AddRange([{ AssetId, ToolId }])
       → context.AssetTags.AddRange([{ AssetId, TagId: X }, { AssetId, TagId: Y }])
       → SaveChangesAsync(ct)  ← single implicit transaction
         → If DbUpdateException with "unique_name" in message: re-throw as ConflictException → 409
       → returns asset.AssetId

    k. Returns asset.AssetId to handler.

6.  CreateAssetCommandHandler returns:
        new PublishAssetResponse { AssetId: <new guid> }

7.  Endpoint returns:
        201 Created
        Location: /api/assets/<new guid>
        { "assetId": "<new guid>" }

8.  Frontend receives 201, clears the localStorage draft, shows a success toast,
    and navigates to /assets/<new guid>.
```

---

## Phase 10 — Frontend Integration

> This section describes integration behavior only. No frontend code is shown.

### Page initialization

When the Publish page loads, the frontend fires **4 GET requests in parallel**:

```
GET /api/assets/types      → populate "Asset Type" dropdown
GET /api/assets/domains    → populate "Domain" dropdown
GET /api/assets/use-cases  → populate "Use Case" dropdown
GET /api/assets/tools      → populate "Compatible AI Tools" multi-select
```

All dropdowns are disabled with a loading spinner until their respective requests complete. Failures show an inline error with a retry button.

### Markdown upload

1. User clicks "Upload .md File" → browser file picker opens (filtered to `.md`).
2. Frontend reads the selected file and sends `multipart/form-data` to `POST /api/assets/upload-markdown`.
3. On success: the `markdownContent` from the response populates the left pane editor immediately.
4. On error (wrong extension, too large, invalid UTF-8): show an inline error near the upload button.

### Split editor + live preview

The publish page uses a **split layout**:

- **Left pane (editor):** Editable `<textarea>` or code editor component. Auto-populated by the upload flow or blank on a fresh page load. The user can type freely.
- **Right pane (preview):** Read-only rendered view. Updates synchronously on every keystroke using a frontend Markdown library (e.g., `marked`, `markdown-it`). No backend calls are made for rendering. The preview supports: headings, paragraphs, lists, tables, code blocks, blockquotes, links, bold, italic. Images are not supported.

### Autosave

The frontend saves the current form state to `localStorage` after a short debounce (~3 seconds after the last keystroke). On page load, if a saved draft is found, the form is restored and a "Restore draft?" banner is shown. The localStorage key is cleared only on a successful publish — not on navigation away, so the draft survives refreshes and tab closures.

### Publish button behavior

- Disabled while any required field is empty.
- Shows a loading spinner while the `POST /api/assets` request is in flight.
- Re-enabled if the request fails.

### Success flow

1. `201 Created` received.
2. Success toast displayed: "Your asset has been published!"
3. `localStorage` draft cleared.
4. User redirected to `/assets/<assetId>` (the new asset detail page).

### Error handling

| Status | Behavior |
|---|---|
| `400` | Parse `error` field from response body; show field-level error(s) near the relevant input |
| `401` | Redirect to login page |
| `409` | Show inline error on `UniqueName` field: "This name is already taken. Please choose a different name." |
| `422` | Show generic error: "One or more selected values are invalid. Please refresh the page and try again." |
| Network error | Show a dismissible banner: "Something went wrong. Please try again." with a Retry button |

---

## Phase 11 — Tests

### Build state

All tests were written to run against the implementation without mocking the database layer in unit tests. All assertions use FluentAssertions.

**Final test results:**
- `WorxForge.Core.UnitTests`: **29 passed, 0 failed**
- `WorxForge.Infrastructure.UnitTests`: **9 passed, 0 failed**

---

### Validator tests — `AssetsValidatorsTests`

**Location:** `src/Tests/UnitTests/WorxForge.Core.UnitTests/Validators/AssetsValidatorsTests.cs`

#### `CreateAssetCommandValidator` tests

| Test | What it verifies |
|---|---|
| `ShouldPass_WhenCommandIsValid` | A fully valid command passes all rules |
| `ShouldThrow_WhenUniqueNameIsEmpty` (`""`, `"   "`) | Empty/whitespace name rejected |
| `ShouldThrow_WhenUniqueNameExceedsMaxLength` (201 chars) | Max length enforced |
| `ShouldThrow_WhenDescriptionIsEmpty` (`""`, `"   "`) | Empty/whitespace description rejected |
| `ShouldThrow_WhenMarkdownContentIsEmpty` | Empty markdown rejected |
| `ShouldThrow_WhenToolIdsIsEmpty` | At least one tool required |
| `ShouldThrow_WhenToolIdsContainsDuplicates` | Duplicate tool IDs rejected |
| `ShouldThrow_WhenTagCountExceedsLimit` (21 tags) | Max 20 tags enforced |
| `ShouldThrow_WhenAnyTagExceedsMaxLength` (51-char tag) | Tag length limit enforced |
| `ShouldThrow_WhenAuthorIdIsEmpty` | Empty Guid rejected |

#### `UploadMarkdownFileCommandValidator` tests

| Test | What it verifies |
|---|---|
| `ShouldPass_WhenFileIsValidMarkdown` (`file.md`, `FILE.MD`, `document.Md`) | Case-insensitive `.md` extension accepted |
| `ShouldThrow_WhenExtensionIsNotMarkdown` (`file.txt`, `file.docx`, `file`) | Non-.md extensions rejected |
| `ShouldThrow_WhenFileIsEmpty` | Empty byte array rejected |
| `ShouldThrow_WhenFileSizeExceedsLimit` (5 MB + 1 byte) | Size limit enforced |

---

### Handler tests — `AssetsHandlersTests`

**Location:** `src/Tests/UnitTests/WorxForge.Core.UnitTests/Handlers/AssetsHandlersTests.cs`

| Test | What it verifies |
|---|---|
| `UploadMarkdownHandler_ShouldReturn_MarkdownContent_WhenFileIsValid` | Valid UTF-8 bytes decoded and returned correctly |
| `UploadMarkdownHandler_ShouldThrow_WhenFileContainsInvalidUtf8` | `0xFF 0xFE` → `ValidationException` with "UTF-8" in message |
| `UploadMarkdownHandler_ShouldThrow_WhenDecodedContentIsEmpty` | Whitespace-only content rejected after trim |
| `CreateAssetHandler_ShouldReturn_AssetId_WhenCommandIsValid` | Handler wraps service result in `PublishAssetResponse` |
| `CreateAssetHandler_ShouldCallValidator_BeforeService` | Validator is called before service (call order assertion) |

---

### Service tests — `AssetServiceTests`

**Location:** `src/Tests/UnitTests/WorxForge.Infrastructure.UnitTests/Configuration/AssetServiceTests.cs`

All dependencies are mocked with Moq. The default `BuildService` helper sets up all repositories to return valid entities, so individual tests only need to override the one repository relevant to the scenario being tested.

| Test | What it verifies |
|---|---|
| `CreateAsync_ShouldThrowConflictException_WhenUniqueNameAlreadyExists` | `ExistsByUniqueNameAsync` returns `true` → `ConflictException` |
| `CreateAsync_ShouldThrowNotFoundException_WhenAssetTypeIdIsInvalid` | `GetByIdAsync` returns `null` → `NotFoundException` |
| `CreateAsync_ShouldThrowNotFoundException_WhenToolIdIsInvalid` | Tool `GetByIdAsync` returns `null` → `NotFoundException` |
| `CreateAsync_ShouldNormalizeTags_ToLowercase` | `"React"`, `"TYPESCRIPT"`, `" CleanArch "` → `["react", "typescript", "cleanarch"]` |
| `CreateAsync_ShouldDeduplicateTags_CaseInsensitively` | `"react"`, `"REACT"`, `"React"` → exactly 1 `GetOrCreate` call |
| `CreateAsync_ShouldDeduplicateToolIds_BeforeSaving` | 3× same toolId → `CreateAsync` called with 1 toolId |
| `CreateAsync_ShouldSkipBlankTags` | `["react", "  ", ""]` → only `"react"` passed to `GetOrCreate` |

---

### DI registration tests — `DependencyInjectionTests`

**Location:** `src/Tests/UnitTests/WorxForge.Infrastructure.UnitTests/Configuration/DependencyInjectionTests.cs`

| Test | What it verifies |
|---|---|
| `AddWorxForgeInfrastructure_ShouldRegisterAssetService` | `IAssetService` resolves from the DI container |
| `AddWorxForgeInfrastructure_ShouldRegisterAssetRepository` | `IAssetRepository` resolves from the DI container |

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| **No new packages** | Hard project constraint |
| **`UniqueName` replaces `Title`** | Assets need a globally unique, permanent identifier; the rename makes the intent explicit in the schema |
| **Unique index at DB level** | Only the database can enforce uniqueness under concurrent load |
| **Race condition caught via `DbUpdateException`** | The application-level check is a fast-fail; the DB index is the real guard |
| **CQRS for every endpoint including upload** | Enforced by the DI convention scanner — no bypass exists |
| **Upload handler works inline, no service** | No DB writes; adding a service method would be an abstraction with no value |
| **Single `SaveChangesAsync` for publish** | EF Core wraps it in an implicit transaction — no explicit `BeginTransaction` needed |
| **Tag normalization in service, not validator** | Normalization is business logic; the validator only checks input format (raw length) |
| **FK validation in service, not validator** | Requires DB access; validators cannot inject repositories per project convention |
| **`LookupItemDto` reused for all 4 lookups** | Already exists; no new DTO types needed |
| **`AuthorId` from JWT, not request body** | Prevents client from impersonating another user |
| **`Version` removed** | Explicitly out of scope |
| **422 for invalid FK references** | 404 would be ambiguous (URL not found vs. entity not found in body); 422 is correct for semantically invalid requests |
| **`.DisableAntiforgery()` on upload** | Bearer-token SPAs do not use antiforgery tokens; the default protection would reject all multipart uploads |
| **`throwOnInvalidBytes: true` for UTF-8 decode** | Rejects corrupted binary files instead of silently storing garbage text |
| **Tool dedup in service (defensive)** | Validator rejects duplicates; service deduplicates again as a safety net before the DB insert |
| **Tags ordered alphabetically in lookups** | Ensures consistent dropdown ordering regardless of insertion order or environment |
