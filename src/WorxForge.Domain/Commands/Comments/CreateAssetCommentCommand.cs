namespace WorxForge.Domain.Commands.Comments;

public sealed class CreateAssetCommentCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
}

