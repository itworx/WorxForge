using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class GetSavedAssetsQueryHandler(IAssetRepository assetRepository)
    : IQueryHandler<GetSavedAssetsQuery, IReadOnlyList<AssetSummaryDto>>
{
    public Task<IReadOnlyList<AssetSummaryDto>> HandleAsync(GetSavedAssetsQuery query, CancellationToken cancellationToken) =>
        assetRepository.GetSavedAssetsAsync(query.UserId, cancellationToken);
}
