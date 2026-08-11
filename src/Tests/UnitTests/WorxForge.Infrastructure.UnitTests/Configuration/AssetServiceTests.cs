using FluentAssertions;
using Moq;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Exceptions;
using WorxForge.Infrastructure.Services;

namespace WorxForge.Infrastructure.UnitTests;

public sealed class AssetServiceTests
{
    private static CreateAssetCommand ValidCommand(IReadOnlyList<Guid>? toolIds = null, IReadOnlyList<string>? tags = null) => new()
    {
        AuthorId = Guid.NewGuid(),
        Title = "my-unique-asset",
        Description = "A description.",
        MarkdownContent = "# Content",
        AssetTypeId = Guid.NewGuid(),
        DomainId = Guid.NewGuid(),
        UseCaseId = Guid.NewGuid(),
        ToolIds = toolIds ?? [Guid.NewGuid()],
        Tags = tags ?? ["React", "typescript"]
    };

    private static AssetService BuildService(
        Mock<IAssetRepository>? assetRepo = null,
        Mock<IAssetTypeRepository>? assetTypeRepo = null,
        Mock<IBusinessDomainRepository>? domainRepo = null,
        Mock<IUseCaseRepository>? useCaseRepo = null,
        Mock<IToolRepository>? toolRepo = null,
        Mock<ITagRepository>? tagRepo = null)
    {
        if (assetRepo is null)
        {
            assetRepo = new Mock<IAssetRepository>();
            assetRepo.Setup(r => r.CreateAsync(It.IsAny<Asset>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Asset a, IEnumerable<Guid> _, IEnumerable<Guid> _, CancellationToken _) => a.AssetId);
        }

        if (assetTypeRepo is null)
        {
            assetTypeRepo = new Mock<IAssetTypeRepository>();
            assetTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new AssetType { AssetTypeId = Guid.NewGuid(), Name = "Prompt" });
        }

        if (domainRepo is null)
        {
            domainRepo = new Mock<IBusinessDomainRepository>();
            domainRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new BusinessDomain { DomainId = Guid.NewGuid(), Name = "Frontend" });
        }

        if (useCaseRepo is null)
        {
            useCaseRepo = new Mock<IUseCaseRepository>();
            useCaseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new UseCase { UseCaseId = Guid.NewGuid(), Name = "Code Generation" });
        }

        if (toolRepo is null)
        {
            toolRepo = new Mock<IToolRepository>();
            toolRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Tool { ToolId = Guid.NewGuid(), Name = "GitHub Copilot" });
        }

        if (tagRepo is null)
        {
            tagRepo = new Mock<ITagRepository>();
            tagRepo.Setup(r => r.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((string name, CancellationToken _) => new Tag { TagId = Guid.NewGuid(), Name = name });
        }

        return new AssetService(assetRepo.Object, assetTypeRepo.Object, domainRepo.Object, useCaseRepo.Object, toolRepo.Object, tagRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenAssetTypeIdIsInvalid()
    {
        var assetTypeRepo = new Mock<IAssetTypeRepository>();
        assetTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((AssetType?)null);

        var service = BuildService(assetTypeRepo: assetTypeRepo);
        var act = () => service.CreateAsync(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenToolIdIsInvalid()
    {
        var toolRepo = new Mock<IToolRepository>();
        toolRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tool?)null);

        var service = BuildService(toolRepo: toolRepo);
        var act = () => service.CreateAsync(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldNormalizeTags_ToLowercase()
    {
        var tagRepo = new Mock<ITagRepository>();
        var capturedNames = new List<string>();
        tagRepo.Setup(r => r.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Callback((string name, CancellationToken _) => capturedNames.Add(name))
               .ReturnsAsync((string name, CancellationToken _) => new Tag { TagId = Guid.NewGuid(), Name = name });

        var service = BuildService(tagRepo: tagRepo);
        await service.CreateAsync(ValidCommand(tags: ["React", "TYPESCRIPT", " CleanArch "]), CancellationToken.None);

        capturedNames.Should().BeEquivalentTo(["react", "typescript", "cleanarch"]);
    }

    [Fact]
    public async Task CreateAsync_ShouldDeduplicateTags_CaseInsensitively()
    {
        var tagRepo = new Mock<ITagRepository>();
        var callCount = 0;
        tagRepo.Setup(r => r.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Callback((string _, CancellationToken _) => callCount++)
               .ReturnsAsync((string name, CancellationToken _) => new Tag { TagId = Guid.NewGuid(), Name = name });

        var service = BuildService(tagRepo: tagRepo);
        await service.CreateAsync(ValidCommand(tags: ["react", "REACT", "React"]), CancellationToken.None);

        // After normalization all three become "react" — only one GetOrCreate call expected.
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldDeduplicateToolIds_BeforeSaving()
    {
        var assetRepo = new Mock<IAssetRepository>();
        IEnumerable<Guid>? savedToolIds = null;
        assetRepo.Setup(r => r.CreateAsync(It.IsAny<Asset>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                 .Callback((Asset _, IEnumerable<Guid> toolIds, IEnumerable<Guid> _, CancellationToken _) => savedToolIds = toolIds)
                 .ReturnsAsync(Guid.NewGuid());

        var toolId = Guid.NewGuid();
        var service = BuildService(assetRepo: assetRepo);
        await service.CreateAsync(ValidCommand(toolIds: [toolId, toolId, toolId]), CancellationToken.None);

        savedToolIds!.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldSkipBlankTags()
    {
        var tagRepo = new Mock<ITagRepository>();
        var capturedNames = new List<string>();
        tagRepo.Setup(r => r.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Callback((string name, CancellationToken _) => capturedNames.Add(name))
               .ReturnsAsync((string name, CancellationToken _) => new Tag { TagId = Guid.NewGuid(), Name = name });

        var service = BuildService(tagRepo: tagRepo);
        await service.CreateAsync(ValidCommand(tags: ["react", "  ", ""]), CancellationToken.None);

        capturedNames.Should().ContainSingle().Which.Should().Be("react");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenAssetDoesNotExist()
    {
        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        var service = BuildService(assetRepo: assetRepo);
        var command = new UpdateAssetCommand
        {
            AssetId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid(),
            Title = "title",
            Description = "description",
            MarkdownContent = "# md",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            ToolIds = [Guid.NewGuid()]
        };

        var act = () => service.UpdateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenUserDoesNotOwnAsset()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = Guid.NewGuid(), AuthorId = ownerId });

        var service = BuildService(assetRepo: assetRepo);
        var command = new UpdateAssetCommand
        {
            AssetId = Guid.NewGuid(),
            RequestingUserId = requesterId,
            Title = "title",
            Description = "description",
            MarkdownContent = "# md",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            ToolIds = [Guid.NewGuid()]
        };

        var act = () => service.UpdateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceRelations_AndPersist()
    {
        var ownerId = Guid.NewGuid();
        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AuthorId = ownerId,
            Title = "old",
            Description = "old",
            MarkdownContent = "old",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdTrackedAsync(asset.AssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        assetRepo.Setup(r => r.UpdateAsync(asset, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var tagRepo = new Mock<ITagRepository>();
        tagRepo.Setup(r => r.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) => new Tag { TagId = Guid.NewGuid(), Name = name });

        var toolId = Guid.NewGuid();
        var service = BuildService(assetRepo: assetRepo, tagRepo: tagRepo);

        await service.UpdateAsync(new UpdateAssetCommand
        {
            AssetId = asset.AssetId,
            RequestingUserId = ownerId,
            Title = "new title",
            Description = "new description",
            MarkdownContent = "# new markdown",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            ToolIds = [toolId, toolId],
            Tags = ["React", " REACT ", "  ", "Api"]
        }, CancellationToken.None);

        asset.Title.Should().Be("new title");
        asset.Description.Should().Be("new description");
        asset.MarkdownContent.Should().Be("# new markdown");
        asset.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        asset.AssetTools.Select(assetTool => assetTool.ToolId).Should().ContainSingle().Which.Should().Be(toolId);
        asset.AssetTags.Should().HaveCount(2);

        assetRepo.Verify(r => r.UpdateAsync(asset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetDeletedAt_WhenOwnedByUser()
    {
        var ownerId = Guid.NewGuid();
        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AuthorId = ownerId,
            Title = "title",
            MarkdownContent = "md",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdTrackedAsync(asset.AssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        assetRepo.Setup(r => r.UpdateAsync(asset, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = BuildService(assetRepo: assetRepo);
        await service.DeleteAsync(asset.AssetId, ownerId, CancellationToken.None);

        asset.DeletedAt.Should().NotBeNull();
        asset.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        assetRepo.Verify(r => r.UpdateAsync(asset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenUserDoesNotOwnAsset()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var asset = new Asset { AssetId = Guid.NewGuid(), AuthorId = ownerId, Title = "title", MarkdownContent = "md" };

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdTrackedAsync(asset.AssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        var service = BuildService(assetRepo: assetRepo);
        var act = () => service.DeleteAsync(asset.AssetId, requesterId, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
