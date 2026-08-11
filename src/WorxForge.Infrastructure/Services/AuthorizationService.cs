using WorxForge.Core.Interfaces.Services;

namespace WorxForge.Infrastructure.Services;

public sealed class AuthorizationService : IAuthorizationService
{
    public Task<bool> CanAccessAssetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<bool> CanEditAssetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<bool> CanDeleteCommentAsync(Guid userId, Guid commentId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
