using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class TagRepository(WorxForgeDbContext dbContext) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Tags.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        if (existing is not null)
            return existing;

        var tag = new Tag { TagId = Guid.NewGuid(), Name = name };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tag;
    }
}
