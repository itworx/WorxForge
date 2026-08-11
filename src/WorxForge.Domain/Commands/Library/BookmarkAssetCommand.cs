namespace WorxForge.Domain.Commands.Library;

public sealed class BookmarkAssetCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
}

