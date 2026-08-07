namespace WorxForge.Domain.Queries.Assets;

public sealed class GetAssetCommentThreadQuery
{
    public Guid AssetId { get; init; }
    public Guid CommentId { get; init; }
}