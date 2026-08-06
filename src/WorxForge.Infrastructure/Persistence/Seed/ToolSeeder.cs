using Microsoft.EntityFrameworkCore;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

public sealed class ToolSeeder : IDataSeeder
{
    public async Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default)
    {
        var existingIds = await context.Tools
            .Select(tool => tool.ToolId)
            .ToHashSetAsync(cancellationToken);

        var existingNames = new HashSet<string>(
            await context.Tools
                .Select(tool => tool.Name)
                .ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        var missingTools = ToolConstants.All
            .Where(entry => !existingIds.Contains(entry.Id) && !existingNames.Contains(entry.Name))
            .Select(entry => new Tool { ToolId = entry.Id, Name = entry.Name })
            .ToList();

        if (missingTools.Count == 0)
        {
            return 0;
        }

        await context.Tools.AddRangeAsync(missingTools, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return missingTools.Count;
    }
}
