using WorxForge.Domain.Dtos.Assets;

namespace WorxForge.Core.Interfaces.Services;

public interface IReactionService
{
    Task<AssetReactionDto> ReactToAssetAsync(Guid userId, Guid assetId, string reactionType, CancellationToken cancellationToken);
    Task<AssetReactionDto> RemoveAssetReactionAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
}
