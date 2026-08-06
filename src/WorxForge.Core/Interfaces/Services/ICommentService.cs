using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;

namespace WorxForge.Core.Interfaces.Services;

public interface ICommentService
{
    Task<PagedResultDto<AssetCommentDto>> GetAssetCommentsAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Guid> CreateCommentAsync(Guid userId, Guid assetId, Guid? parentCommentId, string content, CancellationToken cancellationToken);
    Task DeleteCommentAsync(Guid commentId, Guid requestingUserId, CancellationToken cancellationToken);
}

