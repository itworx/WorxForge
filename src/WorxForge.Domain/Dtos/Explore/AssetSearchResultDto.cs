namespace WorxForge.Domain.Dtos.Explore;

public sealed class AssetSearchResultDto
{
    public Guid AssetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Score { get; init; }
}
