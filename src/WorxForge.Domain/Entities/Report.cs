using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Entities;

public sealed class Report
{
    public Guid ReportId { get; set; }
    public Guid ReporterUserId { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

