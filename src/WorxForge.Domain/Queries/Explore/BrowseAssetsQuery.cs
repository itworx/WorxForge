using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Queries.Explore;

// Client-bindable filters for GET /api/explore/browse; RequestingUserId is never bound from the request.
public sealed class BrowseAssetsFilters
{
    public Guid? AssetTypeId { get; init; }
    public Guid? DomainId { get; init; }
    public Guid? UseCaseId { get; init; }
    public Guid? ToolId { get; init; }
    public AssetFreshness? Freshness { get; init; }
    public string? SortBy { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public sealed class BrowseAssetsQuery
{
    public Guid RequestingUserId { get; init; }
    public Guid? AssetTypeId { get; init; }
    public Guid? DomainId { get; init; }
    public Guid? UseCaseId { get; init; }
    public Guid? ToolId { get; init; }
    public AssetFreshness? Freshness { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}

