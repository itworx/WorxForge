using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class GetSavedAssetsQueryHandler(IAssetRepository assetRepository)
    : IQueryHandler<GetSavedAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public async Task<PagedResultDto<AssetSummaryDto>> HandleAsync(GetSavedAssetsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await assetRepository.GetSavedAssetsAsync(query.UserId, query.Page, query.PageSize, cancellationToken);

        return new PagedResultDto<AssetSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
