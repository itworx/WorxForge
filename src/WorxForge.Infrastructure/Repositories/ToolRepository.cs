using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class ToolRepository(WorxForgeDbContext dbContext) : IToolRepository
{
    public async Task<IReadOnlyList<Tool>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Tools.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Tool?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Tools.AsNoTracking().FirstOrDefaultAsync(tool => tool.ToolId == id, cancellationToken);
}
