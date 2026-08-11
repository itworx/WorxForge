namespace WorxForge.Domain.Queries.Assets;

public sealed class GetAssetDetailsQuery
{
    public Guid AssetId { get; init; }
    public Guid RequestingUserId { get; init; }
}

