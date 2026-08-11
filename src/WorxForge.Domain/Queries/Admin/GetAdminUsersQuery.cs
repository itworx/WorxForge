namespace WorxForge.Domain.Queries.Admin;

public sealed class GetAdminUsersQuery
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

