using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IToolRepository
{
    Task<IReadOnlyList<Tool>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tool?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

