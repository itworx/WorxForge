using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Handlers.Explore;

public sealed class GetExploreFiltersQueryHandler(
    IBusinessDomainRepository businessDomainRepository,
    IAssetTypeRepository assetTypeRepository,
    IUseCaseRepository useCaseRepository,
    IToolRepository toolRepository) : IQueryHandler<GetExploreFiltersQuery, ExploreFiltersDto>
{
    public async Task<ExploreFiltersDto> HandleAsync(GetExploreFiltersQuery query, CancellationToken cancellationToken)
    {
        var domains = await businessDomainRepository.GetAllAsync(cancellationToken);
        var assetTypes = await assetTypeRepository.GetAllAsync(cancellationToken);
        var useCases = await useCaseRepository.GetAllAsync(cancellationToken);
        var tools = await toolRepository.GetAllAsync(cancellationToken);

        return new ExploreFiltersDto
        {
            Domains = domains.Select(domain => new LookupItemDto { Id = domain.DomainId, Name = domain.Name }).ToList(),
            AssetTypes = assetTypes.Select(assetType => new LookupItemDto { Id = assetType.AssetTypeId, Name = assetType.Name }).ToList(),
            UseCases = useCases.Select(useCase => new LookupItemDto { Id = useCase.UseCaseId, Name = useCase.Name }).ToList(),
            Tools = tools.Select(tool => new LookupItemDto { Id = tool.ToolId, Name = tool.Name }).ToList()
        };
    }
}
