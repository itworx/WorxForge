using WorxForge.Domain.Dtos.Home;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IBusinessDomainRepository
{
    Task<IReadOnlyList<BusinessDomain>> GetAllAsync(CancellationToken cancellationToken);
    Task<BusinessDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainSummaryDto>> GetDomainSummariesAsync(CancellationToken cancellationToken);
}

