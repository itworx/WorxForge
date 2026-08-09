using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class GetMyAssetsQueryHandler(IAssetRepository assetRepository)
    : IQueryHandler<GetMyAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public async Task<PagedResultDto<AssetSummaryDto>> HandleAsync(GetMyAssetsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await assetRepository.GetOwnedAssetSummariesAsync(query.UserId, query.Page, query.PageSize, cancellationToken);

        return new PagedResultDto<AssetSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
