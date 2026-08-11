using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class UseCaseRepository(WorxForgeDbContext dbContext) : IUseCaseRepository
{
    public async Task<IReadOnlyList<UseCase>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.UseCases.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<UseCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.UseCases.AsNoTracking().FirstOrDefaultAsync(useCase => useCase.UseCaseId == id, cancellationToken);
}
