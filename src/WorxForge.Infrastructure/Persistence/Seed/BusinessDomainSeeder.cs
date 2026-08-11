using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class BusinessDomainSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.BusinessDomains
            .Select(domain => domain.DomainId)
            .ToHashSetAsync(cancellationToken);

        var existingNames = new HashSet<string>(
            await context.BusinessDomains
                .Select(domain => domain.Name)
                .ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        var missingDomains = BusinessDomainConstants.All
            .Where(entry => !existingIds.Contains(entry.Id) && !existingNames.Contains(entry.Name))
            .Select(entry => new BusinessDomain { DomainId = entry.Id, Name = entry.Name })
            .ToList();

        if (missingDomains.Count == 0)
        {
            return 0;
        }

        await context.BusinessDomains.AddRangeAsync(missingDomains, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingDomains.Count;
    }
}
