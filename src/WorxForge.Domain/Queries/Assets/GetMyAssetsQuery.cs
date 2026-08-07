namespace WorxForge.Domain.Queries.Assets;

public sealed class GetMyAssetsQuery
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

