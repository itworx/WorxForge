namespace WorxForge.Domain.Commands.Comments;

public sealed class RemoveAssetReactionCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
}
