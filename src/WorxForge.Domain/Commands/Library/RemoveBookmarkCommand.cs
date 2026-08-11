namespace WorxForge.Domain.Commands.Library;

public sealed class RemoveBookmarkCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
}

