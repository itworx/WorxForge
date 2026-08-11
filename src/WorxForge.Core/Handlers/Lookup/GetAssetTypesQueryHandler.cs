using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Lookup;

namespace WorxForge.Core.Handlers.Lookup;

public sealed class GetAssetTypesQueryHandler(
    IAssetTypeRepository assetTypeRepository) : IQueryHandler<GetAssetTypesQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(GetAssetTypesQuery query, CancellationToken cancellationToken)
    {
        var types = await assetTypeRepository.GetAllAsync(cancellationToken);
        return types.Select(x => new LookupItemDto { Id = x.AssetTypeId, Name = x.Name }).ToList();
    }
}
