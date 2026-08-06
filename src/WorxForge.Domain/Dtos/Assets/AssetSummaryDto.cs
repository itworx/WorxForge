namespace WorxForge.Domain.Dtos.Assets;

public sealed class AssetSummaryDto
{
    public Guid AssetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string AssetTypeName { get; init; } = string.Empty;
    public string DomainName { get; init; } = string.Empty;
    public IReadOnlyList<string> Tools { get; init; } = [];
    public string AuthorDisplayName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int ReactionCount { get; init; }
    public int CommentCount { get; init; }
    public bool IsBookmarked { get; init; }
}

