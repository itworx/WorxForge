using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Handlers.Explore;

public sealed class SearchAssetsQueryHandler : IQueryHandler<SearchAssetsQuery, PagedResultDto<AssetSummaryDto>>
{
    public Task<PagedResultDto<AssetSummaryDto>> HandleAsync(SearchAssetsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
