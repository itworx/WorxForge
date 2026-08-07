using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class ToolSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.Tools.Select(tool => tool.ToolId).ToHashSetAsync(cancellationToken);

        var missingTools = ToolConstants.All
            .Where(entry => !existingIds.Contains(entry.Id))
            .Select(entry => new Tool { ToolId = entry.Id, Name = entry.Name })
            .ToList();

        await context.Tools.AddRangeAsync(missingTools, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingTools.Count;
    }
}
