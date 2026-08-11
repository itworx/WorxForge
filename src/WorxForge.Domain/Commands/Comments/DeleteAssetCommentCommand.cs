namespace WorxForge.Domain.Commands.Comments;

public sealed class DeleteAssetCommentCommand
{
    public Guid AssetId { get; init; }
    public Guid CommentId { get; init; }
    public Guid RequestingUserId { get; init; }
}

