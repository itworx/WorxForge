using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetViewRepository : IAssetViewRepository
{
    public Task RecordAsync(AssetView view, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<int> GetViewCountAsync(Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
