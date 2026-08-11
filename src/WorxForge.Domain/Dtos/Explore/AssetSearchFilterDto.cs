namespace WorxForge.Domain.Dtos.Explore;

public sealed class AssetSearchFilterDto
{
    public string? Keywords { get; init; }
    public IReadOnlyList<Guid> DomainIds { get; init; } = [];
    public IReadOnlyList<Guid> AssetTypeIds { get; init; } = [];
    public IReadOnlyList<Guid> UseCaseIds { get; init; } = [];
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? SortBy { get; init; }
}

