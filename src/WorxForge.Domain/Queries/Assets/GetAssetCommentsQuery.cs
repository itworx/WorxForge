namespace WorxForge.Domain.Queries.Assets;

public sealed class GetAssetCommentsQuery
{
    public Guid AssetId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

