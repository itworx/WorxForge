namespace WorxForge.Domain.Dtos.Explore;

public sealed class ExploreFiltersDto
{
    public IReadOnlyList<LookupItemDto> Domains { get; init; } = [];
    public IReadOnlyList<LookupItemDto> AssetTypes { get; init; } = [];
    public IReadOnlyList<LookupItemDto> UseCases { get; init; } = [];
    public IReadOnlyList<LookupItemDto> Tools { get; init; } = [];
}

public sealed class LookupItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

