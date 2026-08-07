# Publish Asset

## What changed

This document now captures the final Publish Asset implementation using `Title` again, plus a concise summary of the changes that were made across the stack.

### Code changes

- Added `POST /api/assets` publishing flow in [src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs](../../src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs).
- Updated [src/WorxForge.API/Models/Requests/Assets/CreateAssetRequest.cs](../../src/WorxForge.API/Models/Requests/Assets/CreateAssetRequest.cs) to accept the publish payload.
- Implemented [src/WorxForge.Core/Handlers/Assets/CreateAssetCommandHandler.cs](../../src/WorxForge.Core/Handlers/Assets/CreateAssetCommandHandler.cs) and [src/WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs](../../src/WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs).
- Added publish persistence support in [src/WorxForge.Core/Interfaces/Repositories/IAssetRepository.cs](../../src/WorxForge.Core/Interfaces/Repositories/IAssetRepository.cs), [src/WorxForge.Infrastructure/Repositories/AssetRepository.cs](../../src/WorxForge.Infrastructure/Repositories/AssetRepository.cs), and [src/WorxForge.Infrastructure/Services/AssetService.cs](../../src/WorxForge.Infrastructure/Services/AssetService.cs).
- Added [src/WorxForge.Domain/Dtos/Assets/PublishAssetResponse.cs](../../src/WorxForge.Domain/Dtos/Assets/PublishAssetResponse.cs) and the domain exceptions used by the API layer.
- Added markdown upload support with validation and UTF-8 decoding for the editor flow.
- Kept the asset schema and DTOs on `Title`, not `UniqueName`.

### Data and validation changes

- Asset creation now validates required fields, length limits, tool ID uniqueness, and tag count.
- The service layer checks foreign keys, normalizes tags to lowercase, deduplicates tool IDs defensively, and creates tags on demand.
- The repository saves the asset and join rows in one `SaveChangesAsync` call.
- The API maps validation, conflict, and not-found cases to `400`, `409`, and `422` respectively.

### Frontend-facing behavior

- The publish screen loads lookup data for asset types, domains, use cases, and tools.
- Markdown can be uploaded through `POST /api/assets/upload-markdown` and returned as decoded text.
- Successful publish returns `201 Created` with a `Location` header pointing to the new asset.

## Validation

The focused unit tests for the publish slice passed: `31 passed, 0 failed`.

Relevant checked files:
- [src/WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs](../../src/WorxForge.Core/Validators/Assets/CreateAssetCommandValidator.cs)
- [src/Tests/UnitTests/WorxForge.Core.UnitTests/Validators/AssetsValidatorsTests.cs](../../src/Tests/UnitTests/WorxForge.Core.UnitTests/Validators/AssetsValidatorsTests.cs)
- [src/WorxForge.Infrastructure/Services/AssetService.cs](../../src/WorxForge.Infrastructure/Services/AssetService.cs)
- [src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs](../../src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs)

## Notes

- `Title` is the final canonical field name everywhere in the publish flow.
- No new packages were introduced.
- The old phase-by-phase implementation details were collapsed into this shorter summary to make the document easier to maintain.
