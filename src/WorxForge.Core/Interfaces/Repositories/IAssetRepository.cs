using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken);
    Task<Asset?> GetByIdTrackedAsync(Guid assetId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> SearchAsync(AssetSearchCriteria criteria, CancellationToken cancellationToken);
    Task<AssetDetailsDto?> GetDetailsAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken);
    Task<(IReadOnlyList<MyAssetDto> Items, int Total)> GetByOwnerAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetFeaturedAsync(int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetTrendingAsync(int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetUsefulThisWeekAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetRecentlyUpdatedAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetSavedAssetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetRecommendedForYouAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetPopularAcrossTeamsAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(Asset asset, IEnumerable<Guid> toolIds, IEnumerable<Guid> tagIds, CancellationToken cancellationToken);
    Task AddAsync(Asset asset, CancellationToken cancellationToken);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken);
    Task DeleteAsync(Guid assetId, CancellationToken cancellationToken);
}

