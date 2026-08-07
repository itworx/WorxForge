using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;

namespace WorxForge.Infrastructure.Services;

public sealed class ReactionService(
    IAssetRepository assetRepository,
    IReactionRepository reactionRepository,
    IUserRepository userRepository,
    INotificationService notificationService) : IReactionService
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

            await NotifyAssetOwnerAsync(asset, userId, normalizedReactionType, cancellationToken);
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

    private async Task NotifyAssetOwnerAsync(Asset asset, Guid actorUserId, string normalizedReactionType, CancellationToken cancellationToken)
    {
        if (asset.AuthorId == actorUserId)
        {
            return;
        }

        var actor = await userRepository.GetByIdAsync(actorUserId, cancellationToken);
        var actorDisplayName = actor?.DisplayName ?? "Someone";
        var isLike = string.Equals(normalizedReactionType, ReactionTypes.Like, StringComparison.Ordinal);

        await notificationService.SendAsync(
            recipientUserId: asset.AuthorId,
            actorUserId: actorUserId,
            type: isLike ? NotificationType.PostLiked : NotificationType.PostDisliked,
            message: $"{actorDisplayName} {(isLike ? "liked" : "disliked")} your post \"{asset.Title}\".",
            assetId: asset.AssetId,
            commentId: null,
            cancellationToken: cancellationToken);
    }
}
