using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Errors;
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
        return details ?? throw new NotFoundException(Errors.AssetNotFound);
    }

    public async Task<AssetDetailsDto> GetMyAssetAsync(Guid assetId, Guid ownerId, CancellationToken cancellationToken)
    {
        var details = await GetAssetAsync(assetId, ownerId, cancellationToken);
        if (details.AuthorId != ownerId)
        {
            throw new ForbiddenException(Errors.AssetForbidden);
        }

        return details;
    }
    public Task<PagedResultDto<AssetSummaryDto>> SearchAsync(SearchAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AssetSummaryDto>> BrowseAsync(BrowseAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdTrackedAsync(command.AssetId, cancellationToken)
            ?? throw new NotFoundException(Errors.AssetNotFound);

        EnsureOwnership(asset.AuthorId, command.RequestingUserId);

        await EnsureLookupsExistAsync(command.AssetTypeId, command.DomainId, command.UseCaseId, cancellationToken);

        var toolIds = command.ToolIds.Distinct().ToList();
        await EnsureToolsExistAsync(toolIds, cancellationToken);

        var tagIds = await ResolveTagIdsAsync(command.Tags, cancellationToken);

        asset.Title = command.Title;
        asset.Description = command.Description;
        asset.MarkdownContent = command.MarkdownContent;
        asset.AssetTypeId = command.AssetTypeId;
        asset.DomainId = command.DomainId;
        asset.UseCaseId = command.UseCaseId;
        asset.UpdatedAt = DateTime.UtcNow;

        asset.AssetTools.Clear();
        foreach (var toolId in toolIds)
        {
            asset.AssetTools.Add(new AssetTool { AssetId = asset.AssetId, ToolId = toolId });
        }

        asset.AssetTags.Clear();
        foreach (var tagId in tagIds)
        {
            asset.AssetTags.Add(new AssetTag { AssetId = asset.AssetId, TagId = tagId });
        }

        await assetRepository.UpdateAsync(asset, cancellationToken);
    }

    public async Task DeleteAsync(Guid assetId, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetByIdTrackedAsync(assetId, cancellationToken)
            ?? throw new NotFoundException(Errors.AssetNotFound);

        EnsureOwnership(asset.AuthorId, requestingUserId);

        asset.DeletedAt = DateTime.UtcNow;
        asset.UpdatedAt = DateTime.UtcNow;

        await assetRepository.UpdateAsync(asset, cancellationToken);
    }
    public Task<string> PreviewMarkdownAsync(string markdownContent, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task<Guid> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        await EnsureLookupsExistAsync(command.AssetTypeId, command.DomainId, command.UseCaseId, cancellationToken);

        // Deduplicate tool IDs defensively before FK validation and before saving.
        var toolIds = command.ToolIds.Distinct().ToList();
        await EnsureToolsExistAsync(toolIds, cancellationToken);

        var tagIds = await ResolveTagIdsAsync(command.Tags, cancellationToken);

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

    private async Task EnsureLookupsExistAsync(Guid assetTypeId, Guid domainId, Guid useCaseId, CancellationToken cancellationToken)
    {
        if (await assetTypeRepository.GetByIdAsync(assetTypeId, cancellationToken) is null)
            throw new NotFoundException(Errors.AssetTypeNotFound);

        if (await businessDomainRepository.GetByIdAsync(domainId, cancellationToken) is null)
            throw new NotFoundException(Errors.DomainNotFound);

        if (await useCaseRepository.GetByIdAsync(useCaseId, cancellationToken) is null)
            throw new NotFoundException(Errors.UseCaseNotFound);
    }

    private async Task EnsureToolsExistAsync(IEnumerable<Guid> toolIds, CancellationToken cancellationToken)
    {
        foreach (var toolId in toolIds)
        {
            if (await toolRepository.GetByIdAsync(toolId, cancellationToken) is null)
                throw new NotFoundException(Errors.ToolNotFound);
        }
    }

    private static void EnsureOwnership(Guid authorId, Guid requestingUserId)
    {
        if (authorId != requestingUserId)
            throw new ForbiddenException(Errors.AssetForbidden);
    }

    private async Task<List<Guid>> ResolveTagIdsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken)
    {
        var normalizedTags = tags
            .Select(tag => tag.Trim().ToLowerInvariant())
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var tagIds = new List<Guid>(normalizedTags.Count);
        foreach (var tagName in normalizedTags)
        {
            var tag = await tagRepository.GetOrCreateAsync(tagName, cancellationToken);
            tagIds.Add(tag.TagId);
        }

        return tagIds;
    }
}
