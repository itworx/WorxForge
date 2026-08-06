using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class UseCaseSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.UseCases
            .Select(useCase => useCase.UseCaseId)
            .ToHashSetAsync(cancellationToken);

        var existingNames = new HashSet<string>(
            await context.UseCases
                .Select(useCase => useCase.Name)
                .ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        var missingUseCases = UseCaseConstants.All
            .Where(entry => !existingIds.Contains(entry.Id) && !existingNames.Contains(entry.Name))
            .Select(entry => new UseCase { UseCaseId = entry.Id, Name = entry.Name })
            .ToList();

        if (missingUseCases.Count == 0)
        {
            return 0;
        }

        await context.UseCases.AddRangeAsync(missingUseCases, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingUseCases.Count;
    }
}
