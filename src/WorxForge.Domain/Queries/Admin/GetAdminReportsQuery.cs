using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Queries.Admin;

public sealed class GetAdminReportsQuery
{
    public ReportStatus? Status { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

