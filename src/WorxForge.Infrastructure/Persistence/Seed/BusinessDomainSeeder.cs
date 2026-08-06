using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class BusinessDomainSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.BusinessDomains.Select(domain => domain.DomainId).ToHashSetAsync(cancellationToken);

        var missingDomains = BusinessDomainConstants.All
            .Where(entry => !existingIds.Contains(entry.Id))
            .Select(entry => new BusinessDomain { DomainId = entry.Id, Name = entry.Name })
            .ToList();

        await context.BusinessDomains.AddRangeAsync(missingDomains, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingDomains.Count;
    }
}
