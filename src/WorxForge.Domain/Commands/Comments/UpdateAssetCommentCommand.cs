namespace WorxForge.Domain.Commands.Comments;

public sealed class UpdateAssetCommentCommand
{
    public Guid AssetId { get; init; }
    public Guid CommentId { get; init; }
    public Guid RequestingUserId { get; init; }
    public string Content { get; init; } = string.Empty;
}