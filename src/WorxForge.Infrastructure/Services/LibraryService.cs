using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Library;

namespace WorxForge.Infrastructure.Services;

public sealed class LibraryService : ILibraryService
{
    public Task<PagedResultDto<AssetSummaryDto>> GetSavedAssetsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<LibrarySummaryDto>> GetRecentlyUsedAsync(Guid userId, int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AssetSummaryDto>> GetFollowingAssetsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task BookmarkAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RemoveBookmarkAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
