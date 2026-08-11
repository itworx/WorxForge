using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Queries.Explore;

// Shared client-bindable filters for GET /api/explore/browse and GET /api/assets/search.
// Q is only used by Search; SortBy is only used by Browse - each endpoint reads only what it needs.
public sealed class AssetFilters
{
    public string? Q { get; init; }
    public Guid? AssetTypeId { get; init; }
    public Guid? DomainId { get; init; }
    public Guid? UseCaseId { get; init; }
    public Guid? ToolId { get; init; }
    public AssetFreshness? Freshness { get; init; }
    public string? SortBy { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
