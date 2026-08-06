using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Entities;

namespace WorxForge.Infrastructure.Services;

public sealed class CommentService(IAssetRepository assetRepository, IReactionRepository reactionRepository) : ICommentService
{
    public Task<PagedResultDto<AssetCommentDto>> GetAssetCommentsAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Guid> CreateCommentAsync(Guid userId, Guid assetId, Guid? parentCommentId, string content, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteCommentAsync(Guid commentId, Guid requestingUserId, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task<AssetReactionDto> ReactToAssetAsync(Guid userId, Guid assetId, string reactionType, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdAsync(assetId, cancellationToken);
        if (asset is null)
        {
            throw new KeyNotFoundException($"Asset '{assetId}' was not found.");
        }

        if (asset.AuthorId == userId)
        {
            throw new InvalidOperationException("Users cannot react to their own assets.");
        }

        var normalizedReactionType = ReactionTypes.Normalize(reactionType);
        var existingReaction = await reactionRepository.GetAsync(userId, assetId, cancellationToken);

        if (existingReaction is not null && string.Equals(existingReaction.ReactionType, normalizedReactionType, StringComparison.Ordinal))
        {
            await reactionRepository.DeleteAsync(userId, assetId, cancellationToken);
        }
        else
        {
            await reactionRepository.UpsertAsync(new Reaction
            {
                AssetId = assetId,
                UserId = userId,
                ReactionType = normalizedReactionType
            }, cancellationToken);
        }

        return await reactionRepository.GetSummaryAsync(assetId, userId, cancellationToken);
    }
}
