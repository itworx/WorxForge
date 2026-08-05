using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Queries.Explore;

public sealed class SearchAssetsQuery
{
    // Set by the endpoint from the authenticated user's claims, never bound from client input.
    public Guid RequestingUserId { get; set; }
    public string? Keywords { get; init; }
    public Guid[] DomainIds { get; init; } = [];
    public Guid[] AssetTypeIds { get; init; } = [];
    public Guid[] UseCaseIds { get; init; } = [];
    public Guid[] ToolIds { get; init; } = [];
    public string[] Tags { get; init; } = [];
    public AssetFreshness? Freshness { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

