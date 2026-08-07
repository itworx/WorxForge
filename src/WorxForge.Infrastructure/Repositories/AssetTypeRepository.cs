using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AssetTypeRepository(WorxForgeDbContext dbContext) : IAssetTypeRepository
{
    public async Task<IReadOnlyList<AssetType>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.AssetTypes.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<AssetType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.AssetTypes.AsNoTracking().FirstOrDefaultAsync(assetType => assetType.AssetTypeId == id, cancellationToken);
}
