namespace WorxForge.Domain.Queries.Profile;

public sealed class GetUserAssetsQuery
{
    public Guid TargetUserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
