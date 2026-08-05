using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Home;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class BusinessDomainRepository(WorxForgeDbContext dbContext) : IBusinessDomainRepository
{
    public async Task<IReadOnlyList<BusinessDomain>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.BusinessDomains.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<BusinessDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.BusinessDomains.AsNoTracking().FirstOrDefaultAsync(domain => domain.DomainId == id, cancellationToken);

    public async Task<IReadOnlyList<DomainSummaryDto>> GetDomainSummariesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.BusinessDomains
            .AsNoTracking()
            .Select(domain => new DomainSummaryDto
            {
                DomainId = domain.DomainId,
                Name = domain.Name,
                AssetCount = dbContext.Assets.Count(asset => asset.DomainId == domain.DomainId)
            })
            .ToListAsync(cancellationToken);
    }
}
