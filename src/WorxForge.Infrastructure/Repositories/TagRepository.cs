using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class TagRepository(WorxForgeDbContext dbContext) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Tags.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        var existingTag = await dbContext.Tags.FirstOrDefaultAsync(tag => tag.Name == name, cancellationToken);
        if (existingTag is not null)
        {
            return existingTag;
        }

        var tag = new Tag { TagId = Guid.NewGuid(), Name = name };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tag;
    }
}
