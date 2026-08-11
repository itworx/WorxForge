using WorxForge.Domain.Dtos.Assets;

namespace WorxForge.Domain.Dtos.Library;

public sealed class LibrarySummaryDto
{
    public AssetSummaryDto Asset { get; init; } = new();
    public DateTime LastViewedAt { get; init; }
}

