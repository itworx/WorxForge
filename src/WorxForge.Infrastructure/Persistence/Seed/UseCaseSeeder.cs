using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class UseCaseSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.UseCases.Select(useCase => useCase.UseCaseId).ToHashSetAsync(cancellationToken);

        var missingUseCases = UseCaseConstants.All
            .Where(entry => !existingIds.Contains(entry.Id))
            .Select(entry => new UseCase { UseCaseId = entry.Id, Name = entry.Name })
            .ToList();

        await context.UseCases.AddRangeAsync(missingUseCases, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingUseCases.Count;
    }
}
