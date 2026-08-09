using WorxForge.Domain.Enums;

namespace WorxForge.API.Models.Requests.Admin;

public sealed class ResolveReportByAdminRequest
{
    public ReportStatus NewStatus { get; init; }
}

