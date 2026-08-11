using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Library;

namespace WorxForge.Core.Interfaces.Services;

public interface ILibraryService
{
    Task<PagedResultDto<AssetSummaryDto>> GetSavedAssetsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<LibrarySummaryDto>> GetRecentlyUsedAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<PagedResultDto<AssetSummaryDto>> GetFollowingAssetsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task BookmarkAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
    Task RemoveBookmarkAsync(Guid userId, Guid assetId, CancellationToken cancellationToken);
}

