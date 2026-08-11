namespace WorxForge.Domain.Dtos.Assets;

public sealed class AssetCommentDto
{
    public Guid CommentId { get; init; }
    public Guid AssetId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string AuthorDisplayName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<AssetCommentDto> Replies { get; init; } = [];
}

