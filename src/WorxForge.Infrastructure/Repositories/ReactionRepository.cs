using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class ReactionRepository(WorxForgeDbContext dbContext) : IReactionRepository
{
    public Task<Reaction?> GetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Reactions
            .AsNoTracking()
            .SingleOrDefaultAsync(reaction => reaction.UserId == userId && reaction.AssetId == assetId, cancellationToken);

    public Task<int> CountByAssetAsync(Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Reactions.CountAsync(reaction => reaction.AssetId == assetId, cancellationToken);

    public async Task<AssetReactionDto> GetSummaryAsync(Guid assetId, Guid userId, CancellationToken cancellationToken)
    {
        var reactions = await dbContext.Reactions
            .AsNoTracking()
            .Where(reaction => reaction.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var currentUserReaction = reactions
            .FirstOrDefault(reaction => reaction.UserId == userId)?
            .ReactionType;

        var likeCount = reactions.Count(reaction => string.Equals(reaction.ReactionType, ReactionTypes.Like, StringComparison.Ordinal));
        var dislikeCount = reactions.Count(reaction => string.Equals(reaction.ReactionType, ReactionTypes.Dislike, StringComparison.Ordinal));

        return new AssetReactionDto
        {
            AssetId = assetId,
            LikeCount = likeCount,
            DislikeCount = dislikeCount,
            TotalCount = reactions.Count,
            CurrentUserReaction = currentUserReaction
        };
    }

    public async Task UpsertAsync(Reaction reaction, CancellationToken cancellationToken)
    {
        var existingReaction = await dbContext.Reactions
            .SingleOrDefaultAsync(existing => existing.UserId == reaction.UserId && existing.AssetId == reaction.AssetId, cancellationToken);

        if (existingReaction is null)
        {
            await dbContext.Reactions.AddAsync(reaction, cancellationToken);
        }
        else
        {
            existingReaction.ReactionType = reaction.ReactionType;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid assetId, CancellationToken cancellationToken)
    {
        var existingReaction = await dbContext.Reactions
            .SingleOrDefaultAsync(reaction => reaction.UserId == userId && reaction.AssetId == assetId, cancellationToken);

        if (existingReaction is null)
        {
            return;
        }

        dbContext.Reactions.Remove(existingReaction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
