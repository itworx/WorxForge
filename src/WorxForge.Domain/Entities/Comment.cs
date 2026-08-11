namespace WorxForge.Domain.Entities;

public sealed class Comment
{
    public Guid CommentId { get; set; }
    public Guid AssetId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

