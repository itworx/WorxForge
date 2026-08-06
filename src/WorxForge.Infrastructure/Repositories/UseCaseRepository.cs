using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class UseCaseRepository(WorxForgeDbContext dbContext) : IUseCaseRepository
{
    public async Task<IReadOnlyList<UseCase>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.UseCases.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<UseCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.UseCases.FirstOrDefaultAsync(x => x.UseCaseId == id, cancellationToken);
}
