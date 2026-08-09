using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;
using WorxForge.Domain.Queries.Explore;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetRepository(WorxForgeDbContext dbContext) : IAssetRepository
{
    // Scalar shape of the raw SQL ranking query; only AssetId/Score are needed to determine the
    // page of matching assets and their order, the full AssetSummaryDto is projected separately.
    private sealed record AssetSearchHit(Guid AssetId, double Score);

    public async Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> SearchAsync(AssetSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var (selectBody, parameters) = BuildSearchSql(criteria);

        // "AS Value" is required: EF Core composes SqlQuery<T>().SingleAsync() as a CTE, and
        // SQL Server CTEs require every column - including COUNT(*) - to have a name.
        var countSql = FormattableStringFactory.Create(
            $"SELECT COUNT(*) AS Value FROM ({selectBody}) AS search_results",
            parameters.ToArray());
        var total = await dbContext.Database.SqlQuery<int>(countSql).SingleAsync(cancellationToken);

        var skip = (criteria.Page - 1) * criteria.PageSize;
        var pageParameters = new List<object>(parameters) { skip, criteria.PageSize };
        var pageSql = FormattableStringFactory.Create(
            $"{selectBody} ORDER BY Score DESC, AssetId OFFSET {{{parameters.Count}}} ROWS FETCH NEXT {{{parameters.Count + 1}}} ROWS ONLY",
            pageParameters.ToArray());

        var hits = await dbContext.Database.SqlQuery<AssetSearchHit>(pageSql).ToListAsync(cancellationToken);
        if (hits.Count == 0)
        {
            return ([], total);
        }

        var hitIds = hits.Select(hit => hit.AssetId).ToList();
        var matchedAssets = dbContext.Assets.AsNoTracking().Where(asset => asset.DeletedAt == null && hitIds.Contains(asset.AssetId));
        var summaries = await SelectAssetSummaries(matchedAssets, criteria.RequestingUserId).ToListAsync(cancellationToken);

        // The score ordering computed in SQL is authoritative; re-establish it since the WHERE-IN
        // projection above does not preserve the order of hitIds.
        var rank = hitIds.Select((assetId, index) => (assetId, index)).ToDictionary(x => x.assetId, x => x.index);
        var orderedSummaries = summaries.OrderBy(summary => rank[summary.AssetId]).ToList();

        return (orderedSummaries, total);
    }

    // Builds a plain SQL LIKE search with a SQL-computed relevance score (exact title +100, title contains +50,
    // description contains +20 per term). Case-sensitivity follows the ASSET.title/description column collation,
    // so no LOWER()/UPPER() is applied. Every dynamic value is appended as a "{n}" composite-format placeholder so
    // SqlQuery parameterizes it - never string-concatenated into the SQL text - keeping the query safe from
    // injection even with a variable number of terms/tags/filters.
    private static (string SelectBody, List<object> Parameters) BuildSearchSql(AssetSearchCriteria criteria)
    {
        var parameters = new List<object>();
        var scoreExpr = new System.Text.StringBuilder("CAST(0 AS float)");
        var termConditions = new System.Text.StringBuilder();

        foreach (var term in criteria.Terms)
        {
            parameters.Add(term);
            var exactPlaceholder = $"{{{parameters.Count - 1}}}";
            parameters.Add(EscapeLikePattern(term));
            var likePlaceholder = $"{{{parameters.Count - 1}}}";

            scoreExpr.Append($"""
                 + CASE WHEN a.title = {exactPlaceholder} THEN 100 ELSE 0 END
                 + CASE WHEN a.title LIKE '%' + {likePlaceholder} + '%' ESCAPE '\' THEN 50 ELSE 0 END
                 + CASE WHEN a.description LIKE '%' + {likePlaceholder} + '%' ESCAPE '\' THEN 20 ELSE 0 END
                """);

            termConditions.Append($"""

                AND (a.title LIKE '%' + {likePlaceholder} + '%' ESCAPE '\' OR a.description LIKE '%' + {likePlaceholder} + '%' ESCAPE '\')
                """);
        }

        var sql = new System.Text.StringBuilder();
        sql.Append($"""
            SELECT
                a.asset_id AS AssetId,
                {scoreExpr} AS Score
            FROM ASSET a
            WHERE 1 = 1
            AND a.deleted_at IS NULL
            """);
        sql.Append(termConditions);

        foreach (var tag in criteria.Tags)
        {
            parameters.Add(tag);
            var tagPlaceholder = $"{{{parameters.Count - 1}}}";
            sql.Append($"""

                AND EXISTS (
                    SELECT 1 FROM ASSET_TAG at
                    INNER JOIN TAG t ON t.tag_id = at.tag_id
                    WHERE at.asset_id = a.asset_id AND t.name = {tagPlaceholder}
                )
                """);
        }

        if (criteria.AssetTypeId.HasValue)
        {
            parameters.Add(criteria.AssetTypeId.Value);
            sql.Append($" AND a.asset_type_id = {{{parameters.Count - 1}}}");
        }

        if (criteria.DomainId.HasValue)
        {
            parameters.Add(criteria.DomainId.Value);
            sql.Append($" AND a.domain_id = {{{parameters.Count - 1}}}");
        }

        if (criteria.UseCaseId.HasValue)
        {
            parameters.Add(criteria.UseCaseId.Value);
            sql.Append($" AND a.use_case_id = {{{parameters.Count - 1}}}");
        }

        if (criteria.ToolId.HasValue)
        {
            parameters.Add(criteria.ToolId.Value);
            var toolPlaceholder = $"{{{parameters.Count - 1}}}";
            sql.Append($"""

                AND EXISTS (
                    SELECT 1 FROM ASSET_TOOL att WHERE att.asset_id = a.asset_id AND att.tool_id = {toolPlaceholder}
                )
                """);
        }

        var cutoff = GetFreshnessCutoff(criteria.Freshness);
        if (cutoff.HasValue)
        {
            parameters.Add(cutoff.Value);
            sql.Append($" AND a.created_at >= {{{parameters.Count - 1}}}");
        }

        return (sql.ToString(), parameters);
    }

    // Escapes LIKE wildcard characters in a term value so it matches literally; the value itself
    // is still passed as a query parameter, so this only affects LIKE pattern semantics, not SQL injection safety.
    private static string EscapeLikePattern(string term) =>
        term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

    public async Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken)
    {
        var assets = dbContext.Assets.AsNoTracking().Where(asset => asset.DeletedAt == null);

        if (query.DomainId.HasValue)
        {
            assets = assets.Where(asset => asset.DomainId == query.DomainId.Value);
        }

        if (query.AssetTypeId.HasValue)
        {
            assets = assets.Where(asset => asset.AssetTypeId == query.AssetTypeId.Value);
        }

        if (query.UseCaseId.HasValue)
        {
            assets = assets.Where(asset => asset.UseCaseId == query.UseCaseId.Value);
        }

        if (query.ToolId.HasValue)
        {
            assets = assets.Where(asset => dbContext.AssetTools
                .Any(assetTool => assetTool.AssetId == asset.AssetId && assetTool.ToolId == query.ToolId.Value));
        }

        assets = ApplyFreshnessFilter(assets, query.Freshness);

        var total = await assets.CountAsync(cancellationToken);

        var pagedAssets = ApplySort(assets, query.SortBy, keywords: null)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await SelectAssetSummaries(pagedAssets, query.RequestingUserId).ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(asset => asset.AssetId == assetId && asset.DeletedAt == null, cancellationToken);

    public Task<Asset?> GetByIdTrackedAsync(Guid assetId, CancellationToken cancellationToken) =>
        dbContext.Assets
            .Include(asset => asset.AssetTools)
            .Include(asset => asset.AssetTags)
            .SingleOrDefaultAsync(asset => asset.AssetId == assetId && asset.DeletedAt == null, cancellationToken);

    public async Task<AssetDetailsDto?> GetDetailsAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(existing => existing.AssetId == assetId && existing.DeletedAt == null, cancellationToken);

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
    public async Task<(IReadOnlyList<MyAssetDto> Items, int Total)> GetByOwnerAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var ownedAssets = dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.AuthorId == userId && asset.DeletedAt == null);

        var total = await ownedAssets.CountAsync(cancellationToken);
        if (total == 0)
        {
            return ([], 0);
        }

        var skip = (page - 1) * pageSize;

        var items = await ownedAssets
            .OrderByDescending(asset => asset.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(asset => new MyAssetDto
            {
                AssetId = asset.AssetId,
                Title = asset.Title,
                AssetTypeName = dbContext.AssetTypes
                    .Where(assetType => assetType.AssetTypeId == asset.AssetTypeId)
                    .Select(assetType => assetType.Name)
                    .First(),
                Version = null,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt,
                ViewCount = dbContext.AssetViews
                    .Where(assetView => assetView.AssetId == asset.AssetId)
                    .Sum(assetView => (int?)assetView.ViewCount) ?? 0,
                BookmarkCount = dbContext.Bookmarks.Count(bookmark => bookmark.AssetId == asset.AssetId),
                CommentCount = dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null),
                ReactionCount = dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId)
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }
    public Task<IReadOnlyList<AssetSummaryDto>> GetFeaturedAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IReadOnlyList<AssetSummaryDto>> GetTrendingAsync(int limit, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static IQueryable<Asset> ApplyFreshnessFilter(IQueryable<Asset> assets, AssetFreshness? freshness)
    {
        var cutoff = GetFreshnessCutoff(freshness);
        return cutoff.HasValue ? assets.Where(asset => asset.CreatedAt >= cutoff.Value) : assets;
    }

    private static DateTime? GetFreshnessCutoff(AssetFreshness? freshness) =>
        freshness switch
        {
            AssetFreshness.Week => DateTime.UtcNow.AddDays(-7),
            AssetFreshness.Month => DateTime.UtcNow.AddMonths(-1),
            AssetFreshness.Quarter => DateTime.UtcNow.AddMonths(-3),
            _ => null
        };

    // Relevance falls back to recency when there are no keywords to rank against.
    private IQueryable<Asset> ApplySort(IQueryable<Asset> assets, string? sortBy, string? keywords) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            ExploreSortByConstants.Helpful => assets.OrderByDescending(asset =>
                dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId && reaction.ReactionType == ReactionTypeConstants.Like)),
            ExploreSortByConstants.Updated => assets.OrderByDescending(asset => asset.UpdatedAt),
            ExploreSortByConstants.Discussed => assets.OrderByDescending(asset =>
                dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null)),
            ExploreSortByConstants.Relevance when !string.IsNullOrWhiteSpace(keywords) =>
                assets.OrderByDescending(asset => asset.Title.Contains(keywords) ? 1 : 0).ThenByDescending(asset => asset.CreatedAt),
            _ => assets.OrderByDescending(asset => asset.CreatedAt)
        };

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
            .Where(asset => asset.DeletedAt == null)
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
            .Where(asset => asset.DeletedAt == null)
            .OrderByDescending(asset => asset.UpdatedAt)
            .Take(limit);

        return await SelectAssetSummaries(recentlyUpdatedAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AssetSummaryDto> Items, int Total)> GetSavedAssetsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var savedAssets = dbContext.Bookmarks
            .AsNoTracking()
            .Where(bookmark => bookmark.UserId == userId)
            .Join(dbContext.Assets.AsNoTracking().Where(asset => asset.DeletedAt == null), bookmark => bookmark.AssetId, asset => asset.AssetId, (bookmark, asset) => asset);

        var total = await savedAssets.CountAsync(cancellationToken);
        if (total == 0)
        {
            return ([], 0);
        }

        var skip = (page - 1) * pageSize;

        var pagedAssets = savedAssets
            .OrderByDescending(asset => asset.CreatedAt)
            .Skip(skip)
            .Take(pageSize);

        var items = await SelectAssetSummaries(pagedAssets, userId).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<IReadOnlyList<AssetSummaryDto>> GetRecommendedForYouAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        var likedAssetAffinities = dbContext.Reactions
            .Where(reaction => reaction.UserId == userId && reaction.ReactionType == ReactionTypeConstants.Like)
            .Join(dbContext.Assets.Where(asset => asset.DeletedAt == null), reaction => reaction.AssetId, likedAsset => likedAsset.AssetId,
                (reaction, likedAsset) => new { likedAsset.DomainId, likedAsset.AssetTypeId });

        var bookmarkedAssetAffinities = dbContext.Bookmarks
            .Where(bookmark => bookmark.UserId == userId)
            .Join(dbContext.Assets.Where(asset => asset.DeletedAt == null), bookmark => bookmark.AssetId, bookmarkedAsset => bookmarkedAsset.AssetId,
                (bookmark, bookmarkedAsset) => new { bookmarkedAsset.DomainId, bookmarkedAsset.AssetTypeId });

        var recommendedAssets = dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.DeletedAt == null)
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
            .Where(asset => asset.DeletedAt == null)
            .OrderByDescending(asset =>
                dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId && reaction.ReactionType == ReactionTypeConstants.Like)
                + dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null)
                + dbContext.Bookmarks.Count(bookmark => bookmark.AssetId == asset.AssetId))
            .ThenByDescending(asset => asset.CreatedAt)
            .Take(limit);

        return await SelectAssetSummaries(popularAssets, userId).ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(Asset asset, IEnumerable<Guid> toolIds, IEnumerable<Guid> tagIds, CancellationToken cancellationToken)
    {
        dbContext.Assets.Add(asset);
        dbContext.AssetTools.AddRange(toolIds.Select(id => new AssetTool { AssetId = asset.AssetId, ToolId = id }));
        dbContext.AssetTags.AddRange(tagIds.Select(id => new AssetTag { AssetId = asset.AssetId, TagId = id }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return asset.AssetId;
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken)
    {
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Asset asset, CancellationToken cancellationToken)
    {
        _ = asset;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid assetId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets.SingleOrDefaultAsync(existing => existing.AssetId == assetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        asset.DeletedAt = DateTime.UtcNow;
        asset.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
