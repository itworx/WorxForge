# Comment Module Implementation

This document describes the comment module that was added to the WorxForge backend. The implementation follows the existing Clean Architecture, CQRS, repository/service, and Minimal API conventions already used in the project, and it reuses the current comment infrastructure instead of introducing a separate comment subsystem.

## What Was Implemented

The comment feature now supports the following operations:

- Create a top-level comment on an asset.
- Create a reply to an existing top-level comment.
- Retrieve all comments for an asset, including nested replies.
- Retrieve a single comment thread by comment id.
- Update a comment.
- Soft delete a comment.

The implementation intentionally stays within the existing project structure:

- API routes are wired in the existing asset endpoint group.
- Requests map to CQRS commands and queries.
- Handlers validate requests and delegate to services.
- Services centralize business rules and authorization.
- Repositories handle data access and projection.

## Files Added Or Updated

### Domain

- `src/WorxForge.Domain/Entities/Comment.cs`
  - Added `UpdatedAt` to the entity model.
- `src/WorxForge.Domain/Commands/Comments/UpdateAssetCommentCommand.cs`
  - Added the update command.
- `src/WorxForge.Domain/Commands/Comments/DeleteAssetCommentCommand.cs`
  - Added `AssetId` so delete operations can validate the asset-comment relationship.
- `src/WorxForge.Domain/Queries/Assets/GetAssetCommentThreadQuery.cs`
  - Added the thread query.

### API

- `src/WorxForge.API/Models/Requests/Comments/CreateAssetCommentRequest.cs`
  - Completed the request contract with comment content.
- `src/WorxForge.API/EndpointGroups/AssetsEndpointGroup.cs`
  - Added the comment endpoints and route handlers.

### Core

- `src/WorxForge.Core/Interfaces/Services/ICommentService.cs`
  - Expanded the comment service contract for thread, update, and delete behavior.
- `src/WorxForge.Core/Interfaces/Repositories/ICommentRepository.cs`
  - Added the minimal repository methods needed for thread retrieval and update persistence.
- `src/WorxForge.Core/Validators/Comments/CreateAssetCommentCommandValidator.cs`
- `src/WorxForge.Core/Validators/Comments/DeleteAssetCommentCommandValidator.cs`
- `src/WorxForge.Core/Validators/Comments/UpdateAssetCommentCommandValidator.cs`
- `src/WorxForge.Core/Validators/Assets/GetAssetCommentsQueryValidator.cs`
- `src/WorxForge.Core/Validators/Assets/GetAssetCommentThreadQueryValidator.cs`
  - Added request validation for the new comment flows.
- `src/WorxForge.Core/Handlers/Comments/CreateAssetCommentCommandHandler.cs`
- `src/WorxForge.Core/Handlers/Comments/DeleteAssetCommentCommandHandler.cs`
- `src/WorxForge.Core/Handlers/Comments/UpdateAssetCommentCommandHandler.cs`
- `src/WorxForge.Core/Handlers/Assets/GetAssetCommentsQueryHandler.cs`
- `src/WorxForge.Core/Handlers/Assets/GetAssetCommentThreadQueryHandler.cs`
  - Wired the new CQRS handlers to the service layer.

### Infrastructure

- `src/WorxForge.Infrastructure/Services/CommentService.cs`
  - Implemented comment orchestration, ownership rules, and soft-delete behavior.
- `src/WorxForge.Infrastructure/Repositories/CommentRepository.cs`
  - Implemented EF Core comment reads/writes and nested comment projection.

### Tests

- `src/Tests/UnitTests/WorxForge.Core.UnitTests/Validators/CommentsValidatorsTests.cs`
- `src/Tests/UnitTests/WorxForge.Infrastructure.UnitTests/Configuration/CommentServiceTests.cs`
  - Added unit tests for validation and service behavior.

## Architecture And Design Choices

### 1. Reused The Existing Comment Slice

The repository already contained a partial comment implementation, including:

- `Comment` entity
- `CommentService`
- `CommentRepository`
- comment validators and handlers
- asset comment query/command types
- asset endpoint placeholders

Rather than replacing that slice, the implementation completed it in place. That keeps the code consistent with the rest of the project and avoids unnecessary new abstractions.

### 2. Kept Authorization In The Service Layer

Request validators only check request shape:

- required identifiers
- whitespace content
- maximum content length

Business rules live in `CommentService`:

- asset must exist
- parent comment must exist for replies
- replies can only target top-level comments
- only the owner can edit
- owner or asset owner can delete

This matches the existing asset service pattern, where validation and business logic are kept separate.

### 3. Kept The Repository Contract Small

The repository exposes only the methods needed by the feature:

- get a comment by id
- get all comments for an asset
- get one comment thread
- add a comment
- update a comment
- soft delete a comment

Cascade soft-delete behavior for top-level comments remains internal to the repository. That keeps the service layer simpler and avoids leaking persistence details into the public contract.

### 4. Projected Directly To DTOs For Reads

For read operations, the repository builds nested `AssetCommentDto` results directly.

That approach was chosen to:

- avoid unnecessary `Include`s
- reduce N+1 risk
- keep read models aligned with API response models
- preserve stable comment/reply ordering

Top-level comments are returned as the primary list items, and replies are nested under each parent comment.

### 5. Enforced Two-Level Comment Hierarchy

The system supports exactly two levels:

- top-level comment
- reply

Replies cannot be nested under replies. If a client tries to reply to a reply, the service rejects the request with a validation exception.

### 6. Soft Delete Is Applied Consistently

Deleted comments are filtered out in all read paths.

Soft-delete behavior works as follows:

- deleting a top-level comment soft-deletes the top-level comment and its direct replies
- deleting a reply soft-deletes only that reply
- deleted comments remain in the database
- deleted comments never appear in API responses

### 7. UpdatedAt Is Maintained

`UpdatedAt` stays null on create and is only populated when the comment is edited or deleted.

The service updates it when:

- a comment is created
- a comment is edited
- a comment is soft-deleted

## API Surface

The comment routes are mounted under the existing asset group:

- `GET /api/assets/{assetId}/comments`
- `GET /api/assets/{assetId}/comments/{commentId}`
- `POST /api/assets/{assetId}/comments`
- `POST /api/assets/{assetId}/comments/{commentId}/replies`
- `PUT /api/assets/{assetId}/comments/{commentId}`
- `DELETE /api/assets/{assetId}/comments/{commentId}`

All comment routes require the authenticated user policy.

### Route Behavior

- `GET /api/assets/{assetId}/comments`
  - returns paged top-level comments with nested replies
- `GET /api/assets/{assetId}/comments/{commentId}`
  - returns a single comment thread rooted at the target top-level comment
- `POST /api/assets/{assetId}/comments`
  - creates a top-level comment
- `POST /api/assets/{assetId}/comments/{commentId}/replies`
  - creates a reply to the specified top-level comment
- `PUT /api/assets/{assetId}/comments/{commentId}`
  - updates comment content
- `DELETE /api/assets/{assetId}/comments/{commentId}`
  - soft-deletes the comment

## Validation Rules

The validators enforce request-shape rules only.

### Create Comment

- `UserId` required
- `AssetId` required
- `ParentCommentId`, if supplied, cannot be empty
- `Content` required
- `Content` cannot be whitespace
- `Content` maximum length is 2000 characters

### Update Comment

- `AssetId` required
- `CommentId` required
- `RequestingUserId` required
- `Content` required
- `Content` cannot be whitespace
- `Content` maximum length is 2000 characters

### Delete Comment

- `AssetId` required
- `CommentId` required
- `RequestingUserId` required

### Comment Retrieval

- `AssetId` required
- `Page` must be at least 1
- `PageSize` must be at least 1
- `PageSize` is capped at 100

## Data Access Strategy

The repository uses direct projection for comment reads. It loads the minimal fields needed to build `AssetCommentDto` instances and then assembles the nested tree in memory.

This keeps the code straightforward while still avoiding a heavy EF graph-loading approach.

Read paths also consistently exclude rows where `DeletedAt` is set.

## Soft Delete And Thread Projection

When a top-level comment is deleted:

1. the service decides that the delete should cascade to the thread
2. the top-level comment and its direct replies are marked deleted
3. all affected rows get `DeletedAt` and `UpdatedAt` set to the current UTC time
4. the next read hides them automatically

When a reply is deleted:

1. only that reply is marked deleted
2. the parent top-level comment remains visible

### Thread Endpoint Rule

The thread endpoint only accepts top-level comments. If a client asks for a thread using a reply id, the service rejects it with a validation exception.

## Database Notes

The existing COMMENT table was reused.

The codebase already had `UpdatedAt` in the model snapshot, so no schema migration was needed for this work.
The comment module intentionally keeps cascade-delete decisions in the service layer, which matches the asset edit/delete flow and keeps repository methods focused on data access.

The implementation did not add any of the following:

- `ReplyToCommentId`
- `ReplyToUserId`
- edit history tables
- versioning tables
- reaction tables
- notification tables
- report tables

## Testing

Two focused unit test files were added:

- `CommentsValidatorsTests`
- `CommentServiceTests`

These cover:

- content validation
- whitespace rejection
- max length validation
- reply-to-reply rejection
- top-level comment creation
- update timestamp behavior
- delete authorization behavior
- thread retrieval behavior

### Validation Run

The following checks were run successfully:

- `dotnet test` for `WorxForge.Core.UnitTests` filtered to `CommentsValidatorsTests`
- `dotnet test` for `WorxForge.Infrastructure.UnitTests` filtered to `CommentServiceTests`
- `dotnet build` for `WorxForge.API`

## Summary

The comment module now fits into the existing backend architecture without introducing a parallel design.

It uses:

- the existing comment entity and EF model
- the existing CQRS handler style
- the existing validation and exception pattern
- the existing service/repository split
- the existing asset endpoint group

The result is a small, maintainable implementation that supports nested comments, replies, update, delete, and thread retrieval while keeping the read and write behavior consistent across the API.