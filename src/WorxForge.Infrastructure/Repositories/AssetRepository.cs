using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Queries.Explore;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetRepository(WorxForgeDbContext dbContext) : IAssetRepository
{
    public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(asset => asset.AssetId == assetId, cancellationToken);

    public async Task<AssetDetailsDto?> GetDetailsAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(existing => existing.AssetId == assetId, cancellationToken);

        if (asset is null)
        {
            return null;
        }

        var assetTypeName = await dbContext.AssetTypes
            .AsNoTracking()
            .Where(assetType => assetType.AssetTypeId == asset.AssetTypeId)
            .Select(assetType => assetType.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var domainName = await dbContext.BusinessDomains
            .AsNoTracking()
            .Where(domain => domain.DomainId == asset.DomainId)
            .Select(domain => domain.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var useCaseName = await dbContext.UseCases
            .AsNoTracking()
            .Where(useCase => useCase.UseCaseId == asset.UseCaseId)
            .Select(useCase => useCase.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var author = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserId == asset.AuthorId)
            .Select(user => new { user.DisplayName, user.AvatarUrl })
            .SingleOrDefaultAsync(cancellationToken);

        var compatibleTools = await dbContext.AssetTools
            .AsNoTracking()
            .Where(assetTool => assetTool.AssetId == asset.AssetId)
            .Join(dbContext.Tools, assetTool => assetTool.ToolId, tool => tool.ToolId, (assetTool, tool) => tool.Name)
            .ToListAsync(cancellationToken);

        var tags = await dbContext.AssetTags
            .AsNoTracking()
            .Where(assetTag => assetTag.AssetId == asset.AssetId)
            .Join(dbContext.Tags, assetTag => assetTag.TagId, tag => tag.TagId, (assetTag, tag) => tag.Name)
            .ToListAsync(cancellationToken);

        var reactionCount = await dbContext.Reactions
            .AsNoTracking()
            .CountAsync(reaction => reaction.AssetId == asset.AssetId, cancellationToken);

        var commentCount = await dbContext.Comments
            .AsNoTracking()
            .CountAsync(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null, cancellationToken);

        var viewCount = await dbContext.AssetViews
            .AsNoTracking()
            .Where(assetView => assetView.AssetId == asset.AssetId)
            .SumAsync(assetView => (int?)assetView.ViewCount, cancellationToken) ?? 0;

        var isBookmarked = await dbContext.Bookmarks
            .AsNoTracking()
            .AnyAsync(bookmark => bookmark.AssetId == asset.AssetId && bookmark.UserId == requestingUserId, cancellationToken);

        return new AssetDetailsDto
        {
            AssetId = asset.AssetId,
            Title = asset.Title,
            Description = asset.Description,
            MarkdownContent = asset.MarkdownContent,
            AssetTypeName = assetTypeName,
            DomainName = domainName,
            UseCaseName = useCaseName,
            CompatibleTools = compatibleTools,
            Tags = tags,
            Version = null,
            AuthorId = asset.AuthorId,
            AuthorDisplayName = author?.DisplayName ?? string.Empty,
            AuthorAvatarUrl = author?.AvatarUrl,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt,
            ReactionCount = reactionCount,
            CommentCount = commentCount,
            ViewCount = viewCount,
            IsBookmarked = isBookmarked
        };
    }
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> SearchAsync(SearchAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetByOwnerAsync(Guid userId, int page, int pageSize, string? sortBy, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetFeaturedAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetTrendingAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();

    // Shared projection so every "browse" style query maps Asset -> AssetSummaryDto identically;
    // callers apply their own Where/OrderBy/Take before projecting.
    private IQueryable<AssetSummaryDto> SelectAssetSummaries(IQueryable<Asset> assets, Guid userId) =>
        assets.Select(asset => new AssetSummaryDto
        {
            AssetId = asset.AssetId,
            Title = asset.Title,
            Description = asset.Description,
            AssetTypeName = dbContext.AssetTypes
                .Where(assetType => assetType.AssetTypeId == asset.AssetTypeId)
                .Select(assetType => assetType.Name)
                .First(),
            DomainName = dbContext.BusinessDomains
                .Where(domain => domain.DomainId == asset.DomainId)
                .Select(domain => domain.Name)
                .First(),
            Tools = dbContext.AssetTools
                .Where(assetTool => assetTool.AssetId == asset.AssetId)
                .Join(dbContext.Tools, assetTool => assetTool.ToolId, tool => tool.ToolId, (assetTool, tool) => tool.Name)
                .ToList(),
            AuthorDisplayName = dbContext.Users
                .Where(user => user.UserId == asset.AuthorId)
                .Select(user => user.DisplayName)
                .First(),
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt,
            ReactionCount = dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId),
            CommentCount = dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null),
            IsBookmarked = dbContext.Bookmarks.Any(bookmark => bookmark.AssetId == asset.AssetId && bookmark.UserId == userId)
        });

    public async Task<IReadOnlyList<AssetSummaryDto>> GetUsefulThisWeekAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddDays(-7);

        var recentlyLikedAssets = dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.CreatedAt >= since)
            .OrderByDescending(asset => dbContext.Reactions.Count(reaction =>
                reaction.AssetId == asset.AssetId && reaction.ReactionType == ReactionTypeConstants.Like))
            .Take(limit);

        return await SelectAssetSummaries(recentlyLikedAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetSummaryDto>> GetRecentlyUpdatedAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        var recentlyUpdatedAssets = dbContext.Assets
            .AsNoTracking()
            .OrderByDescending(asset => asset.UpdatedAt)
            .Take(limit);

        return await SelectAssetSummaries(recentlyUpdatedAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetSummaryDto>> GetSavedAssetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var savedAssets = dbContext.Bookmarks
            .AsNoTracking()
            .Where(bookmark => bookmark.UserId == userId)
            .Join(dbContext.Assets.AsNoTracking(), bookmark => bookmark.AssetId, asset => asset.AssetId, (bookmark, asset) => asset);

        return await SelectAssetSummaries(savedAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetSummaryDto>> GetRecommendedForYouAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        var likedAssetAffinities = dbContext.Reactions
            .Where(reaction => reaction.UserId == userId && reaction.ReactionType == ReactionTypeConstants.Like)
            .Join(dbContext.Assets, reaction => reaction.AssetId, likedAsset => likedAsset.AssetId,
                (reaction, likedAsset) => new { likedAsset.DomainId, likedAsset.AssetTypeId });

        var bookmarkedAssetAffinities = dbContext.Bookmarks
            .Where(bookmark => bookmark.UserId == userId)
            .Join(dbContext.Assets, bookmark => bookmark.AssetId, bookmarkedAsset => bookmarkedAsset.AssetId,
                (bookmark, bookmarkedAsset) => new { bookmarkedAsset.DomainId, bookmarkedAsset.AssetTypeId });

        var recommendedAssets = dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.AuthorId != userId)
            .Where(asset => !dbContext.Reactions.Any(reaction =>
                reaction.AssetId == asset.AssetId && reaction.UserId == userId && reaction.ReactionType == ReactionTypeConstants.Like))
            .Where(asset => !dbContext.Bookmarks.Any(bookmark => bookmark.AssetId == asset.AssetId && bookmark.UserId == userId))
            .OrderByDescending(asset =>
                ((likedAssetAffinities.Count(affinity => affinity.DomainId == asset.DomainId) * AssetRecommendationConstants.LikeWeight)
                    + (bookmarkedAssetAffinities.Count(affinity => affinity.DomainId == asset.DomainId) * AssetRecommendationConstants.BookmarkWeight)) * AssetRecommendationConstants.DomainMatchWeight
                + ((likedAssetAffinities.Count(affinity => affinity.AssetTypeId == asset.AssetTypeId) * AssetRecommendationConstants.LikeWeight)
                    + (bookmarkedAssetAffinities.Count(affinity => affinity.AssetTypeId == asset.AssetTypeId) * AssetRecommendationConstants.BookmarkWeight)) * AssetRecommendationConstants.TypeMatchWeight)
            .ThenByDescending(asset => dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId))
            .ThenByDescending(asset => asset.CreatedAt)
            .Take(limit);

        // Already-liked/bookmarked assets are excluded above, so the shared projection's IsBookmarked is always false here.
        return await SelectAssetSummaries(recommendedAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetSummaryDto>> GetPopularAcrossTeamsAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        var popularAssets = dbContext.Assets
            .AsNoTracking()
            .OrderByDescending(asset =>
                dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId && reaction.ReactionType == ReactionTypeConstants.Like)
                + dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null)
                + dbContext.Bookmarks.Count(bookmark => bookmark.AssetId == asset.AssetId))
            .ThenByDescending(asset => asset.CreatedAt)
            .Take(limit);

        return await SelectAssetSummaries(popularAssets, userId).ToListAsync(cancellationToken);
    }

    public Task AddAsync(Asset asset, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateAsync(Asset asset, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAsync(Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
