namespace WorxForge.Domain.Queries.Admin;

public sealed class GetAdminCommentsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

