using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Infrastructure.Services;

public sealed class AssetService(
    IAssetRepository assetRepository,
    IAssetTypeRepository assetTypeRepository,
    IBusinessDomainRepository businessDomainRepository,
    IUseCaseRepository useCaseRepository,
    IToolRepository toolRepository,
    ITagRepository tagRepository) : IAssetService
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
    public Task UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<string> PreviewMarkdownAsync(string markdownContent, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task<Guid> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        if (await assetTypeRepository.GetByIdAsync(command.AssetTypeId, cancellationToken) is null)
            throw new NotFoundException($"Asset type '{command.AssetTypeId}' was not found.");

        if (await businessDomainRepository.GetByIdAsync(command.DomainId, cancellationToken) is null)
            throw new NotFoundException($"Domain '{command.DomainId}' was not found.");

        if (await useCaseRepository.GetByIdAsync(command.UseCaseId, cancellationToken) is null)
            throw new NotFoundException($"Use case '{command.UseCaseId}' was not found.");

        // Deduplicate tool IDs defensively before FK validation and before saving.
        var toolIds = command.ToolIds.Distinct().ToList();

        foreach (var toolId in toolIds)
        {
            if (await toolRepository.GetByIdAsync(toolId, cancellationToken) is null)
                throw new NotFoundException($"Tool '{toolId}' was not found.");
        }

        var normalizedTags = command.Tags
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var tagIds = new List<Guid>(normalizedTags.Count);
        foreach (var tagName in normalizedTags)
        {
            var tag = await tagRepository.GetOrCreateAsync(tagName, cancellationToken);
            tagIds.Add(tag.TagId);
        }

        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            Title = command.Title,
            Description = command.Description,
            MarkdownContent = command.MarkdownContent,
            AssetTypeId = command.AssetTypeId,
            DomainId = command.DomainId,
            UseCaseId = command.UseCaseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await assetRepository.CreateAsync(asset, toolIds, tagIds, cancellationToken);
    }
}
