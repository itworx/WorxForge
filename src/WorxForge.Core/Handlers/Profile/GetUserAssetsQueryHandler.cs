using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Handlers.Profile;

public sealed class GetUserAssetsQueryHandler(IAssetRepository assetRepository)
    : IQueryHandler<GetUserAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public async Task<PagedResultDto<AssetSummaryDto>> HandleAsync(GetUserAssetsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await assetRepository.GetOwnedAssetSummariesAsync(query.TargetUserId, query.Page, query.PageSize, cancellationToken);

        return new PagedResultDto<AssetSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
