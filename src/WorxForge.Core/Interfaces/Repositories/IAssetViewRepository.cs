using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IAssetViewRepository
{
    Task RecordAsync(AssetView view, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetSummaryDto>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<int> GetViewCountAsync(Guid assetId, CancellationToken cancellationToken);
}

