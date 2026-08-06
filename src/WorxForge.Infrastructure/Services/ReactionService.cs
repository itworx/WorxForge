using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Infrastructure.Services;

public sealed class ReactionService(IAssetRepository assetRepository, IReactionRepository reactionRepository) : IReactionService
{
    public async Task<AssetReactionDto> ReactToAssetAsync(Guid userId, Guid assetId, string reactionType, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdAsync(assetId, cancellationToken);
        if (asset is null)
        {
            throw new KeyNotFoundException($"Asset '{assetId}' was not found.");
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

    public async Task<AssetReactionDto> RemoveAssetReactionAsync(Guid userId, Guid assetId, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdAsync(assetId, cancellationToken);
        if (asset is null)
        {
            throw new KeyNotFoundException($"Asset '{assetId}' was not found.");
        }

        await reactionRepository.DeleteAsync(userId, assetId, cancellationToken);
        return await reactionRepository.GetSummaryAsync(assetId, userId, cancellationToken);
    }
}
