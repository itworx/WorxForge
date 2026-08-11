using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IReactionRepository
{
    Task<Reaction?> GetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
    Task<int> CountByAssetAsync(Guid assetId, CancellationToken cancellationToken);
    Task<AssetReactionDto> GetSummaryAsync(Guid assetId, Guid userId, CancellationToken cancellationToken);
    Task UpsertAsync(Reaction reaction, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
}

