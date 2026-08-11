namespace WorxForge.Domain.Commands.Comments;

// TODO: Confirm reaction type taxonomy with product owner.
public sealed class ReactToAssetCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
    public string ReactionType { get; init; } = string.Empty;
}

