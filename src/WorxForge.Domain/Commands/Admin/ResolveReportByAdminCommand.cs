using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Commands.Admin;

public sealed class ResolveReportByAdminCommand
{
    public Guid ReportId { get; init; }
    public Guid AdminUserId { get; init; }
    public ReportStatus NewStatus { get; init; }
}

