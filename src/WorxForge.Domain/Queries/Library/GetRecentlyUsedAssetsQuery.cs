namespace WorxForge.Domain.Queries.Library;

public sealed class GetRecentlyUsedAssetsQuery
{
    public Guid UserId { get; init; }
    public int Limit { get; init; } = 10;
}

