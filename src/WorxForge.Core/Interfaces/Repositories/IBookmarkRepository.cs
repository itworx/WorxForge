using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IBookmarkRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
}

