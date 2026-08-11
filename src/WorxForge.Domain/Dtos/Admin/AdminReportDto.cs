using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Dtos.Admin;

public sealed class AdminReportDto
{
    public Guid ReportId { get; init; }
    public string ReporterUsername { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public ReportStatus Status { get; init; }
    public Guid? AssetId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

