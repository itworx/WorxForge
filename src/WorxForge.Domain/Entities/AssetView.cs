namespace WorxForge.Domain.Entities;

public sealed class AssetView
{
    public Guid UserId { get; set; }
    public Guid AssetId { get; set; }
    public DateTime LastViewedAt { get; set; }
    public int ViewCount { get; set; }
}

