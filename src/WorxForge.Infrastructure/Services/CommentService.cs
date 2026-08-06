using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;

namespace WorxForge.Infrastructure.Services;

public sealed class CommentService : ICommentService
{
    public Task<PagedResultDto<AssetCommentDto>> GetAssetCommentsAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Guid> CreateCommentAsync(Guid userId, Guid assetId, Guid? parentCommentId, string content, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteCommentAsync(Guid commentId, Guid requestingUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
