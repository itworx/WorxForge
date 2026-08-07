using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AssetCommentDto> Items, int Total)> GetByAssetAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AssetCommentDto> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<AssetCommentDto?> GetThreadAsync(Guid assetId, Guid commentId, CancellationToken cancellationToken);
    Task AddAsync(Comment comment, CancellationToken cancellationToken);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken);
    Task SoftDeleteAsync(IReadOnlyCollection<Guid> commentIds, CancellationToken cancellationToken);
}

