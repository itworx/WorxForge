namespace WorxForge.Domain.Dtos.Admin;

public sealed class AdminCommentDto
{
    public Guid CommentId { get; init; }
    public string AuthorUsername { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int ReportCount { get; init; }
}

