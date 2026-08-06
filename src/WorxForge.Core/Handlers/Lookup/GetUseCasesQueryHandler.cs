using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Lookup;

namespace WorxForge.Core.Handlers.Lookup;

public sealed class GetUseCasesQueryHandler(
    IUseCaseRepository useCaseRepository) : IQueryHandler<GetUseCasesQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(GetUseCasesQuery query, CancellationToken cancellationToken)
    {
        var useCases = await useCaseRepository.GetAllAsync(cancellationToken);
        return useCases.Select(x => new LookupItemDto { Id = x.UseCaseId, Name = x.Name }).ToList();
    }
}
