using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class GetAdminReportsQueryHandler(IReportRepository reportRepository) : IQueryHandler<GetAdminReportsQuery, PagedResultDto<AdminReportDto>>
{
    public async Task<PagedResultDto<AdminReportDto>> HandleAsync(GetAdminReportsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await reportRepository.GetAllAsync(query.Status, query.SortBy, query.Page, query.PageSize, cancellationToken);

        return new PagedResultDto<AdminReportDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
