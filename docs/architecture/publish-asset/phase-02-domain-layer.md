# Phase 2 — Domain Layer

## Goal

Update all domain types to reflect the `Title → UniqueName` rename, remove `Version` (out of scope), and introduce the new commands, queries, DTOs, and exception types needed for the Publish Asset feature.

---

## Entity Changes

### `WorxForge.Domain/Entities/Asset.cs`

Before:
```csharp
public sealed class Asset
{
    public Guid AssetId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MarkdownContent { get; set; } = string.Empty;
    public Guid AssetTypeId { get; set; }
    public Guid DomainId { get; set; }
    public Guid UseCaseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

After:
```csharp
public sealed class Asset
{
    public Guid AssetId { get; set; }
    public Guid AuthorId { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;  // no longer nullable
    public string MarkdownContent { get; set; } = string.Empty;
    public Guid AssetTypeId { get; set; }
    public Guid DomainId { get; set; }
    public Guid UseCaseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Changes:**
- `Title` → `UniqueName`
- `Description?` → `Description` (non-nullable; required by the publish screen)

---

## Command Changes

### `CreateAssetCommand.cs`

Before:
```csharp
public sealed class CreateAssetCommand
{
    public Guid AuthorId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid AssetTypeId { get; init; }
    public Guid DomainId { get; init; }
    public Guid UseCaseId { get; init; }
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Version { get; init; }
    public string MarkdownContent { get; init; } = string.Empty;
}
```

After:
```csharp
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

**Changes:**
- `Title` → `UniqueName`
- `Description?` → `Description` (required)
- `Version` removed (out of scope)

### `UpdateAssetCommand.cs`

Same renames applied: `Title` → `UniqueName`, `Description?` → `Description`, `Version` removed. This keeps the update command consistent with the create command even though `UpdateAsset` is not yet implemented.

---

## DTO Changes

All DTOs that exposed `Title` and `Version` were updated for consistency.

| DTO | Change |
|---|---|
| `AssetDetailsDto` | `Title` → `UniqueName`; `Description?` → `Description`; `Version` removed |
| `AssetSummaryDto` | `Title` → `UniqueName`; `Description?` → `Description` |
| `MyAssetDto` | `Title` → `UniqueName`; `Version` removed |
| `AdminAssetDto` | `Title` → `UniqueName` |

---

## New Command

### `UploadMarkdownFileCommand.cs`

```csharp
public sealed class UploadMarkdownFileCommand
{
    public byte[] FileContent { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
}
```

**Purpose:** Carries the raw bytes of the uploaded file and its original filename to the handler. The handler validates the extension and decodes the bytes as UTF-8.

**Why `byte[]` instead of `IFormFile`?** Commands must be pure data — they should not carry HTTP abstractions like `IFormFile`. The endpoint reads the file into a `byte[]` before creating the command.

---

## New Queries

Four parameterless queries for loading reference data. Each returns `IReadOnlyList<LookupItemDto>`, which already exists in the codebase.

```csharp
// WorxForge.Domain/Queries/Lookup/
public sealed class GetAssetTypesQuery;
public sealed class GetDomainsQuery;
public sealed class GetUseCasesQuery;
public sealed class GetToolsQuery;
```

These queries carry no parameters because all reference data is returned in full (the lists are small and seeded).

---

## New Response DTOs

### `PublishAssetResponse.cs`

```csharp
public sealed class PublishAssetResponse
{
    public Guid AssetId { get; init; }
}
```

Returned by `CreateAssetCommandHandler` after a successful publish. The API layer uses this to build the `Location` header.

### `UploadMarkdownResponse.cs`

```csharp
public sealed class UploadMarkdownResponse
{
    public string MarkdownContent { get; init; } = string.Empty;
}
```

Returned by `UploadMarkdownFileCommandHandler`. The frontend uses this to populate the markdown editor.

---

## New Exception Types

Three typed exceptions were added under `WorxForge.Domain/Exceptions/`, all inheriting from the existing `WorxForgeException`:

```csharp
// Maps to HTTP 400 — invalid input format or content
public sealed class ValidationException(string message) : WorxForgeException(message);

// Maps to HTTP 409 — resource with the same unique identifier already exists
public sealed class ConflictException(string message) : WorxForgeException(message);

// Maps to HTTP 422 — valid request referencing a resource that does not exist
public sealed class NotFoundException(string message) : WorxForgeException(message);
```

**Why not use ASP.NET's `ProblemDetails` or `HttpException`?** The domain layer must not reference ASP.NET. These exceptions are pure domain types. The API layer (`ExceptionMiddleware`) maps them to HTTP status codes.

**Why 422 for `NotFoundException` instead of 404?** A 404 typically means "the endpoint URL was not found." When a client submits a valid request body referencing a `DomainId` that does not exist, the request itself is semantically wrong — which is 422 Unprocessable Entity. 404 would be ambiguous.

---

## Reused Types

| Type | Namespace | Reused For |
|---|---|---|
| `LookupItemDto { Guid Id; string Name; }` | `WorxForge.Domain.Dtos.Explore` | All 4 lookup query responses |
| `AssetTool`, `AssetTag` | `WorxForge.Domain.Entities` | Junction rows created during publish |
