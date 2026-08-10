namespace WorxForge.Domain.Dtos.Assets;

public sealed class AssetDetailsDto
{
    public Guid AssetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string MarkdownContent { get; init; } = string.Empty;
    public string AssetTypeName { get; init; } = string.Empty;
    public string DomainName { get; init; } = string.Empty;
    public string UseCaseName { get; init; } = string.Empty;
    public IReadOnlyList<string> CompatibleTools { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Version { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorDisplayName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int CommentCount { get; init; }
    public int ViewCount { get; init; }
    public bool IsBookmarked { get; init; }
    public int LikeCount { get; init; }
    public int DislikeCount { get; init; }
    public string? CurrentUserReaction { get; init; }
}

