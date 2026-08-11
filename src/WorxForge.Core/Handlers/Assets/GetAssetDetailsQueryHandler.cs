using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class GetAssetDetailsQueryHandler(IAssetService assetService) : IQueryHandler<GetAssetDetailsQuery, WorxForge.Domain.Dtos.Assets.AssetDetailsDto>
{
    public Task<WorxForge.Domain.Dtos.Assets.AssetDetailsDto> HandleAsync(GetAssetDetailsQuery query, CancellationToken cancellationToken) =>
        assetService.GetAssetAsync(query.AssetId, query.RequestingUserId, cancellationToken);
}
