using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Commands.Reports;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;

namespace WorxForge.Core.Handlers.Reports;

public sealed class ReportAssetCommandHandler(
    IAssetRepository assetRepository,
    IReportRepository reportRepository) : ICommandHandler<ReportAssetCommand>
{
    public async Task HandleAsync(ReportAssetCommand command, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdAsync(command.AssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset '{command.AssetId}' was not found.");

        var reportId = Guid.NewGuid();

        var report = new Report
        {
            ReportId = reportId,
            ReporterUserId = command.UserId,
            Reason = command.Reason,
            Status = ReportStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        var reportAsset = new ReportAsset
        {
            ReportId = reportId,
            AssetId = asset.AssetId
        };

        await reportRepository.AddAssetReportAsync(report, reportAsset, cancellationToken);
    }
}
