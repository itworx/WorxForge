namespace WorxForge.Domain.Queries.Admin;

public sealed class GetAdminAssetsQuery
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

