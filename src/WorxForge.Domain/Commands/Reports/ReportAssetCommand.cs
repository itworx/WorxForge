namespace WorxForge.Domain.Commands.Reports;

public sealed class ReportAssetCommand
{
    public Guid UserId { get; init; }
    public Guid AssetId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
