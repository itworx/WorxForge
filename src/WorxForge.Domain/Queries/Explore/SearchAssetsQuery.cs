using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Queries.Explore;

public sealed class SearchAssetsQuery
{
    // Set by the endpoint from the authenticated user's claims, never bound from client input.
    public Guid RequestingUserId { get; set; }
    public string? Q { get; init; }
    public Guid? AssetTypeId { get; init; }
    public Guid? DomainId { get; init; }
    public Guid? UseCaseId { get; init; }
    public Guid? ToolId { get; init; }
    public AssetFreshness? Freshness { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Repository-facing search criteria: parsing of Q into FTS terms/hashtags already happened in the handler.
public sealed class AssetSearchCriteria
{
    public Guid RequestingUserId { get; init; }
    public IReadOnlyList<string> Terms { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public Guid? AssetTypeId { get; init; }
    public Guid? DomainId { get; init; }
    public Guid? UseCaseId { get; init; }
    public Guid? ToolId { get; init; }
    public AssetFreshness? Freshness { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}

