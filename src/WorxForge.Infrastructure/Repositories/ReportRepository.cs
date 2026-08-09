using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class ReportRepository(WorxForgeDbContext dbContext) : IReportRepository
{
    public Task<Report?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken) =>
        dbContext.Reports.SingleOrDefaultAsync(report => report.ReportId == reportId, cancellationToken);

    public async Task AddAsync(Report report, CancellationToken cancellationToken)
    {
        await dbContext.Reports.AddAsync(report, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAssetReportAsync(Report report, ReportAsset reportAsset, CancellationToken cancellationToken)
    {
        await dbContext.Reports.AddAsync(report, cancellationToken);
        await dbContext.ReportAssets.AddAsync(reportAsset, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Report report, CancellationToken cancellationToken)
    {
        dbContext.Reports.Update(report);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AdminReportDto> Items, int Total)> GetAllAsync(
        ReportStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Reports.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(report => report.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        if (total == 0)
        {
            return ([], 0);
        }

        query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "status"         => query.OrderBy(report => report.Status),
            "status_desc"    => query.OrderByDescending(report => report.Status),
            "createdat"      => query.OrderBy(report => report.CreatedAt),
            "createdat_desc" => query.OrderByDescending(report => report.CreatedAt),
            "resolvedat"     => query.OrderBy(report => report.ResolvedAt),
            "resolvedat_desc"=> query.OrderByDescending(report => report.ResolvedAt),
            _                => query.OrderByDescending(report => report.CreatedAt)
        };

        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Join(dbContext.Users.AsNoTracking(), report => report.ReporterUserId, user => user.UserId, (report, user) => new { report, user })
            .GroupJoin(dbContext.ReportAssets.AsNoTracking(), x => x.report.ReportId, ra => ra.ReportId, (x, ra) => new { x.report, x.user, ra })
            .SelectMany(x => x.ra.DefaultIfEmpty(), (x, ra) => new AdminReportDto
            {
                ReportId         = x.report.ReportId,
                ReporterUsername = x.user.Username,
                Reason           = x.report.Reason,
                Status           = x.report.Status,
                AssetId          = ra != null ? ra.AssetId : (Guid?)null,
                CreatedAt        = x.report.CreatedAt,
                ResolvedAt       = x.report.ResolvedAt
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
