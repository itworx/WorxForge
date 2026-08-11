using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class GetMyAssetDetailsQueryHandler : IQueryHandler<GetMyAssetDetailsQuery, WorxForge.Domain.Dtos.Assets.AssetDetailsDto>
{
    public Task<WorxForge.Domain.Dtos.Assets.AssetDetailsDto> HandleAsync(GetMyAssetDetailsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
