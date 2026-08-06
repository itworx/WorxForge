using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class AssetTypeSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.AssetTypes.Select(assetType => assetType.AssetTypeId).ToHashSetAsync(cancellationToken);

        var missingAssetTypes = AssetTypeConstants.All
            .Where(entry => !existingIds.Contains(entry.Id))
            .Select(entry => new AssetType { AssetTypeId = entry.Id, Name = entry.Name })
            .ToList();

        await context.AssetTypes.AddRangeAsync(missingAssetTypes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingAssetTypes.Count;
    }
}
