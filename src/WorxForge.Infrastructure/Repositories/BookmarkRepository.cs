using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class BookmarkRepository(WorxForgeDbContext dbContext) : IBookmarkRepository
{
    public Task<bool> ExistsAsync(Guid userId, Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Bookmarks
            .AsNoTracking()
            .AnyAsync(bookmark => bookmark.UserId == userId && bookmark.AssetId == assetId, cancellationToken);

    // GetSavedAssetsAsync in AssetRepository already handles My Library retrieval.
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken)
    {
        await dbContext.Bookmarks.AddAsync(bookmark, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid assetId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Bookmarks
            .SingleOrDefaultAsync(bookmark => bookmark.UserId == userId && bookmark.AssetId == assetId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        dbContext.Bookmarks.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
