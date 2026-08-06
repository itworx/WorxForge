using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Queries.Explore;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetRepository(WorxForgeDbContext dbContext) : IAssetRepository
{
    public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(asset => asset.AssetId == assetId, cancellationToken);

    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> SearchAsync(SearchAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetByOwnerAsync(Guid userId, int page, int pageSize, string? sortBy, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetFeaturedAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetTrendingAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(Asset asset, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateAsync(Asset asset, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAsync(Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
