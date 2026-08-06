namespace WorxForge.Domain.Dtos.Home;

public sealed class DomainSummaryDto
{
    public Guid DomainId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int AssetCount { get; init; }
}
