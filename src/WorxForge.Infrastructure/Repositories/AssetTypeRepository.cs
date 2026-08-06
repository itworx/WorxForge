using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetTypeRepository(WorxForgeDbContext dbContext) : IAssetTypeRepository
{
    public async Task<IReadOnlyList<AssetType>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.AssetTypes.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<AssetType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AssetTypes.FirstOrDefaultAsync(x => x.AssetTypeId == id, cancellationToken);
}
