namespace WorxForge.Domain.Queries.Assets;

public sealed class GetMyAssetDetailsQuery
{
    public Guid AssetId { get; init; }
    public Guid OwnerId { get; init; }
}

