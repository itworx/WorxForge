using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<AdminReportDto> Items, int Total)> GetAllAsync(ReportStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Report report, CancellationToken cancellationToken);
    Task AddAssetReportAsync(Report report, ReportAsset reportAsset, CancellationToken cancellationToken);
    Task UpdateAsync(Report report, CancellationToken cancellationToken);
}

