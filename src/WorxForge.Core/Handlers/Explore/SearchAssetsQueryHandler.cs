using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Search;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Handlers.Explore;

public sealed class SearchAssetsQueryHandler(IAssetRepository assetRepository) : IQueryHandler<SearchAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public async Task<PagedResultDto<AssetSummaryDto>> HandleAsync(SearchAssetsQuery query, CancellationToken cancellationToken)
    {
        var parsed = SearchQueryParser.Parse(query.Q);

        var criteria = new AssetSearchCriteria
        {
            RequestingUserId = query.RequestingUserId,
            Terms = parsed.Terms,
            Tags = parsed.Hashtags,
            AssetTypeId = query.AssetTypeId,
            DomainId = query.DomainId,
            UseCaseId = query.UseCaseId,
            ToolId = query.ToolId,
            Freshness = query.Freshness,
            Page = query.Page,
            PageSize = query.PageSize
        };

        var (items, total) = await assetRepository.SearchAsync(criteria, cancellationToken);

        return new PagedResultDto<AssetSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
