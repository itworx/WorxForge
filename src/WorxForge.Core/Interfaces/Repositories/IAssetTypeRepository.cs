using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface IAssetTypeRepository
{
    Task<IReadOnlyList<AssetType>> GetAllAsync(CancellationToken cancellationToken);
    Task<AssetType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

