using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Handlers.Explore;

public sealed class BrowseAssetsQueryHandler(IAssetRepository assetRepository) : IQueryHandler<BrowseAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public async Task<PagedResultDto<AssetSummaryDto>> HandleAsync(BrowseAssetsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await assetRepository.BrowseAsync(query, cancellationToken);

        return new PagedResultDto<AssetSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
