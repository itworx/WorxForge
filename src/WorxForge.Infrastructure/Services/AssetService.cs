using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Infrastructure.Services;

public sealed class AssetService(IAssetRepository assetRepository) : IAssetService
{
    public async Task<AssetDetailsDto> GetAssetAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var details = await assetRepository.GetDetailsAsync(assetId, requestingUserId, cancellationToken);
        return details ?? throw new KeyNotFoundException($"Asset '{assetId}' was not found.");
    }

    public async Task<AssetDetailsDto> GetMyAssetAsync(Guid assetId, Guid ownerId, CancellationToken cancellationToken)
    {
        var details = await GetAssetAsync(assetId, ownerId, cancellationToken);
        if (details.AuthorId != ownerId)
        {
            throw new UnauthorizedAccessException("You are not allowed to access this asset as owner.");
        }

        return details;
    }
    public Task<PagedResultDto<MyAssetDto>> GetMyAssetsAsync(Guid userId, int page, int pageSize, string? sortBy, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AssetSummaryDto>> SearchAsync(SearchAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AssetSummaryDto>> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Guid> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<string> PreviewMarkdownAsync(string markdownContent, CancellationToken cancellationToken) => throw new NotImplementedException();
}
