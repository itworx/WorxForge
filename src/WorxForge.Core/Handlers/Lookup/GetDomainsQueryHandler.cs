using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Lookup;

namespace WorxForge.Core.Handlers.Lookup;

public sealed class GetDomainsQueryHandler(
    IBusinessDomainRepository domainRepository) : IQueryHandler<GetDomainsQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(GetDomainsQuery query, CancellationToken cancellationToken)
    {
        var domains = await domainRepository.GetAllAsync(cancellationToken);
        return domains.Select(x => new LookupItemDto { Id = x.DomainId, Name = x.Name }).ToList();
    }
}
