namespace WorxForge.Domain.Queries.Library;

public sealed class GetFollowingAssetsQuery
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

