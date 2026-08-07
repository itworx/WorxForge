using WorxForge.Core.Interfaces.Services;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Infrastructure.Services;

public sealed class CommentService(
    ICommentRepository commentRepository,
    IAssetRepository assetRepository) : ICommentService
{
    public async Task<PagedResultDto<AssetCommentDto>> GetAssetCommentsAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await EnsureAssetExistsAsync(assetId, cancellationToken);

        var (items, totalCount) = await commentRepository.GetByAssetAsync(assetId, page, pageSize, cancellationToken);
        return new PagedResultDto<AssetCommentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AssetCommentDto> GetAssetCommentThreadAsync(Guid assetId, Guid commentId, CancellationToken cancellationToken)
    {
            await EnsureAssetExistsAsync(assetId, cancellationToken);

            var comment = await commentRepository.GetByIdAsync(commentId, cancellationToken)
                ?? throw new NotFoundException($"Comment '{commentId}' was not found.");

            if (comment.AssetId != assetId)
            {
                throw new NotFoundException($"Comment '{commentId}' was not found.");
            }

            if (comment.ParentCommentId.HasValue)
            {
                throw new ValidationException("Thread endpoint only accepts top-level comments.");
            }

        return await commentRepository.GetThreadAsync(assetId, commentId, cancellationToken)
            ?? throw new NotFoundException($"Comment '{commentId}' was not found.");
    }

    public async Task<Guid> CreateCommentAsync(Guid userId, Guid assetId, Guid? parentCommentId, string content, CancellationToken cancellationToken)
    {
        await EnsureAssetExistsAsync(assetId, cancellationToken);

        if (parentCommentId.HasValue)
        {
            var parentComment = await commentRepository.GetByIdAsync(parentCommentId.Value, cancellationToken)
                ?? throw new NotFoundException($"Comment '{parentCommentId}' was not found.");

            if (parentComment.AssetId != assetId)
            {
                throw new NotFoundException($"Comment '{parentCommentId}' was not found.");
            }

            if (parentComment.ParentCommentId.HasValue)
            {
                throw new ValidationException("Replies may only target top-level comments.");
            }
        }

        var nowUtc = DateTime.UtcNow;
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            AssetId = assetId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            Content = content,
            CreatedAt = nowUtc,
            UpdatedAt = null
        };

        await commentRepository.AddAsync(comment, cancellationToken);
        return comment.CommentId;
    }

    public async Task UpdateCommentAsync(Guid assetId, Guid commentId, Guid requestingUserId, string content, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, cancellationToken)
            ?? throw new NotFoundException($"Comment '{commentId}' was not found.");

        if (comment.AssetId != assetId)
        {
            throw new NotFoundException($"Comment '{commentId}' was not found.");
        }

        EnsureOwnership(comment.UserId, requestingUserId);

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;

        await commentRepository.UpdateAsync(comment, cancellationToken);
    }

    public async Task DeleteCommentAsync(Guid assetId, Guid commentId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, cancellationToken)
            ?? throw new NotFoundException($"Comment '{commentId}' was not found.");

        if (comment.AssetId != assetId)
        {
            throw new NotFoundException($"Comment '{commentId}' was not found.");
        }

        var asset = await EnsureAssetExistsAsync(assetId, cancellationToken);

        if (comment.UserId != requestingUserId && asset.AuthorId != requestingUserId)
        {
            throw new ForbiddenException("You are not allowed to delete this comment.");
        }

        var commentIdsToDelete = new List<Guid> { commentId };

        if (comment.ParentCommentId is null)
        {
            var thread = await commentRepository.GetThreadAsync(assetId, commentId, cancellationToken)
                ?? throw new NotFoundException($"Comment '{commentId}' was not found.");

            commentIdsToDelete.AddRange(thread.Replies.Select(reply => reply.CommentId));
        }

        await commentRepository.SoftDeleteAsync(commentIdsToDelete, cancellationToken);
    }

    private async Task<Asset> EnsureAssetExistsAsync(Guid assetId, CancellationToken cancellationToken) =>
        await assetRepository.GetByIdAsync(assetId, cancellationToken)
            ?? throw new NotFoundException($"Asset '{assetId}' was not found.");

    private static void EnsureOwnership(Guid authorId, Guid requestingUserId)
    {
        if (authorId != requestingUserId)
        {
            throw new ForbiddenException("You are not allowed to modify this comment.");
        }
    }
}
