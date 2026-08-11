using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Interfaces.Services;

public interface IAssetService
{
    Task<AssetDetailsDto> GetAssetAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken);
    Task<AssetDetailsDto> GetMyAssetAsync(Guid assetId, Guid ownerId, CancellationToken cancellationToken);
    Task<PagedResultDto<AssetSummaryDto>> SearchAsync(SearchAssetsQuery query, CancellationToken cancellationToken);
    Task<PagedResultDto<AssetSummaryDto>> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken);
    Task<string> PreviewMarkdownAsync(string markdownContent, CancellationToken cancellationToken);
}

