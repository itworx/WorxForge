using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IUseCaseRepository
{
    Task<IReadOnlyList<UseCase>> GetAllAsync(CancellationToken cancellationToken);
    Task<UseCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

