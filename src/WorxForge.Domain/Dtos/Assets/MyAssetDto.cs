namespace WorxForge.Domain.Dtos.Assets;

public sealed class MyAssetDto
{
    public Guid AssetId { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public string AssetTypeName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int ViewCount { get; init; }
    public int BookmarkCount { get; init; }
    public int CommentCount { get; init; }
    public int ReactionCount { get; init; }
}

