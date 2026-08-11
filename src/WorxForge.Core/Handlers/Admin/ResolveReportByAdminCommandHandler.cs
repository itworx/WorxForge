using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Commands.Admin;
using WorxForge.Domain.Enums;

namespace WorxForge.Core.Handlers.Admin;

public sealed class ResolveReportByAdminCommandHandler(IReportRepository reportRepository) : ICommandHandler<ResolveReportByAdminCommand>
{
    public async Task HandleAsync(ResolveReportByAdminCommand command, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(command.ReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report '{command.ReportId}' was not found.");

        report.Status = command.NewStatus;

        if (command.NewStatus is ReportStatus.Resolved or ReportStatus.Rejected)
        {
            report.ResolvedBy = command.AdminUserId;
            report.ResolvedAt = DateTime.UtcNow;
        }
        else
        {
            report.ResolvedBy = null;
            report.ResolvedAt = null;
        }

        await reportRepository.UpdateAsync(report, cancellationToken);
    }
}
