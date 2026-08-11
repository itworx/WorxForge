namespace WorxForge.Domain.Commands.Assets;

public sealed class DeleteAssetCommand
{
    public Guid AssetId { get; init; }
    public Guid RequestingUserId { get; init; }
}

