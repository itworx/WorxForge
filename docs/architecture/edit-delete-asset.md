# Edit & Delete Asset - Implementation Summary

## Overview

This document summarizes the implemented **Edit Asset** and **Delete Asset** flows, following the same project conventions used by Publish Asset and Authentication:

- Clean Architecture layering
- CQRS (request -> validator -> handler -> service -> repository)
- JWT-based user identity extraction
- Explicit ownership checks
- Soft-delete behavior for assets

## Endpoints

### 1) Edit Asset

- **Method/Route:** `PUT /api/assets/{assetId}`
- **Auth:** Required (`AuthenticatedUser` policy)
- **Behavior:**
  - Extract requesting user ID from JWT `sub` claim.
  - Map route/body to `UpdateAssetCommand`.
  - Validate command.
  - Verify ownership and update asset data.
  - Replace tool/tag relationships in tracked workflow.
  - Return success response.

### 2) Delete Asset

- **Method/Route:** `DELETE /api/assets/{assetId}`
- **Auth:** Required (`AuthenticatedUser` policy)
- **Behavior:**
  - Extract requesting user ID from JWT `sub` claim.
  - Map route to `DeleteAssetCommand`.
  - Validate command.
  - Verify ownership.
  - Soft-delete by setting `DeletedAt` and updating `UpdatedAt`.
  - Return success response.

## Request Models

- `UpdateAssetRequest` is kept as an **independent request model** (not inheriting from create request).
- `CreateAssetRequest` remains separate.

## Validation

### UpdateAssetCommandValidator

Implemented directly (without mapping to create command), using equivalent validation rules:

- AssetId required
- RequestingUserId required
- Title required, max length
- Description required, max length
- MarkdownContent required
- AssetTypeId, DomainId, UseCaseId required
- ToolIds: at least one, unique
- Tags: max count and max length per tag

### DeleteAssetCommandValidator

- AssetId required
- RequestingUserId required

## Service Layer Responsibilities

### Update

- Load tracked asset
- Ensure asset exists
- Ensure requester owns the asset
- Validate foreign keys for asset type/domain/use case/tools
- Normalize tags (`trim + lowercase + remove blanks + distinct`)
- Reuse tag `GetOrCreateAsync`
- Update scalar fields
- Update join relationships via tracked workflow
- Set `UpdatedAt`
- Persist using one save path

### Delete

- Load tracked asset
- Ensure asset exists
- Ensure requester owns the asset
- Set `DeletedAt = DateTime.UtcNow`
- Set `UpdatedAt = DateTime.UtcNow`
- Persist using one save path

## Soft Delete Strategy

`Asset` now supports soft delete via nullable `DeletedAt`.

- Deleted assets are filtered out from normal read surfaces (details/search/browse/profile/recommendation-related reads).
- This preserves data for auditing and avoids hard-deletion side effects.

### Why This Matters For Comments

The comment module follows the same ownership and soft-delete philosophy:

- delete decisions stay in the service layer
- the repository handles persistence, not authorization
- related rows are filtered out of reads instead of being physically removed

Keeping these rules aligned avoids two different deletion models in the same backend.

## Error Handling

- Added `ForbiddenException` for ownership failures.
- Exception middleware maps forbidden cases to **HTTP 403**.
- Existing project-wide behavior for not-found remains unchanged.

## Files Added

- `src/WorxForge.Domain/Exceptions/ForbiddenException.cs`
- `src/WorxForge.Infrastructure/Migrations/20260807181823_AddAssetSoftDelete.cs`
- `src/WorxForge.Infrastructure/Migrations/20260807181823_AddAssetSoftDelete.Designer.cs`

## Key Files Updated

- `src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs`
- `src/WorxForge.API/Models/Requests/Assets/UpdateAssetRequest.cs`
- `src/WorxForge.Core/Validators/Assets/UpdateAssetCommandValidator.cs`
- `src/WorxForge.Core/Validators/Assets/DeleteAssetCommandValidator.cs`
- `src/WorxForge.Core/Handlers/Assets/UpdateAssetCommandHandler.cs`
- `src/WorxForge.Core/Handlers/Assets/DeleteAssetCommandHandler.cs`
- `src/WorxForge.Infrastructure/Services/AssetService.cs`
- `src/WorxForge.Core/Interfaces/Repositories/IAssetRepository.cs`
- `src/WorxForge.Infrastructure/Repositories/AssetRepository.cs`
- `src/WorxForge.Domain/Entities/Asset.cs`
- `src/WorxForge.API/Middlewares/ExceptionMiddleware.cs`
- `src/WorxForge.Infrastructure/Configuration/WorxForgeDbContext.cs`
- `src/WorxForge.Infrastructure/Migrations/WorxForgeDbContextModelSnapshot.cs`

## Testing and Verification

Focused verification steps used:

1. `dotnet build src/WorxForge.slnx`
2. `dotnet test src/Tests/UnitTests/WorxForge.Core.UnitTests/WorxForge.Core.UnitTests.csproj --filter "Assets"`
3. `dotnet test src/Tests/UnitTests/WorxForge.Infrastructure.UnitTests/WorxForge.Infrastructure.UnitTests.csproj --filter "AssetService"`

These checks validated:

- validator behavior
- handler orchestration
- ownership rules
- update/delete service behavior
- soft-delete semantics

## Final Notes

The implementation keeps the same architecture and coding conventions used in existing slices, avoids unnecessary repository API expansion, and preserves a clear CQRS flow with explicit ownership and validation boundaries.
