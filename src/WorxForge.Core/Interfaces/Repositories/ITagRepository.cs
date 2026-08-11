using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken);
}

