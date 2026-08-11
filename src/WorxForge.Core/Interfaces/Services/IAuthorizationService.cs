namespace WorxForge.Core.Interfaces.Services;

public interface IAuthorizationService
{
    Task<bool> CanAccessAssetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
    Task<bool> CanEditAssetAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
    Task<bool> CanDeleteCommentAsync(Guid userId, Guid commentId, CancellationToken cancellationToken);
    Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken);
}

