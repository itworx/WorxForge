using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetFollowRepository(WorxForgeDbContext dbContext) : IAssetFollowRepository
{
    public Task<bool> ExistsAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) =>
        dbContext.AssetFollows.AnyAsync(assetFollow => assetFollow.UserId == userId && assetFollow.AssetId == assetId, cancellationToken);

    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetFollowedByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task AddAsync(Guid userId, Guid assetId, CancellationToken cancellationToken)
    {
        if (await ExistsAsync(userId, assetId, cancellationToken))
        {
            return;
        }

        await dbContext.AssetFollows.AddAsync(new AssetFollow
        {
            UserId = userId,
            AssetId = assetId
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid assetId, CancellationToken cancellationToken)
    {
        var existingFollow = await dbContext.AssetFollows
            .SingleOrDefaultAsync(assetFollow => assetFollow.UserId == userId && assetFollow.AssetId == assetId, cancellationToken);

        if (existingFollow is null)
        {
            return;
        }

        dbContext.AssetFollows.Remove(existingFollow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
