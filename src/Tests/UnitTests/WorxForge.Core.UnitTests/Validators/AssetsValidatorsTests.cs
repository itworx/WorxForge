using FluentAssertions;
using WorxForge.Core.Validators.Assets;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.UnitTests;

public sealed class AssetsValidatorsTests
{
    // -- CreateAssetCommandValidator -------------------------------------------

    private static CreateAssetCommand ValidCreateCommand(
        string title = "my-asset",
        string description = "A valid description.",
        string markdownContent = "# Hello",
        int toolCount = 1,
        string[]? tags = null,
        Guid? authorId = null) => new()
    {
        AuthorId = authorId ?? Guid.NewGuid(),
        Title = title,
        Description = description,
        MarkdownContent = markdownContent,
        AssetTypeId = Guid.NewGuid(),
        DomainId = Guid.NewGuid(),
        UseCaseId = Guid.NewGuid(),
        ToolIds = Enumerable.Range(0, toolCount).Select(_ => Guid.NewGuid()).ToList(),
        Tags = tags ?? ["react", "typescript"]
    };

    [Fact]
    public async Task CreateAssetValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAssetValidator_ShouldThrow_WhenTitleIsEmpty(string name)
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(title: name), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenTitleExceedsMaxLength()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(title: new string('x', 201)), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAssetValidator_ShouldThrow_WhenDescriptionIsEmpty(string description)
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(description: description), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenMarkdownContentIsEmpty()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(markdownContent: ""), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenToolIdsIsEmpty()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(toolCount: 0), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenToolIdsContainsDuplicates()
    {
        var validator = new CreateAssetCommandValidator();
        var toolId = Guid.NewGuid();
        var command = new CreateAssetCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "asset",
            Description = "desc",
            MarkdownContent = "# md",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            ToolIds = [toolId, toolId]
        };
        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenTagCountExceedsLimit()
    {
        var validator = new CreateAssetCommandValidator();
        var tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToArray();
        var act = () => validator.ValidateAsync(ValidCreateCommand(tags: tags), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenAnyTagExceedsMaxLength()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(tags: [new string('a', 51)]), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAssetValidator_ShouldThrow_WhenAuthorIdIsEmpty()
    {
        var validator = new CreateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(authorId: Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    // -- UpdateAssetCommandValidator -------------------------------------------

    private static UpdateAssetCommand ValidUpdateCommand(
        Guid? assetId = null,
        Guid? requestingUserId = null,
        string title = "my-asset",
        string description = "A valid description.",
        string markdownContent = "# Hello",
        IReadOnlyList<Guid>? toolIds = null,
        IReadOnlyList<string>? tags = null) => new()
    {
        AssetId = assetId ?? Guid.NewGuid(),
        RequestingUserId = requestingUserId ?? Guid.NewGuid(),
        Title = title,
        Description = description,
        MarkdownContent = markdownContent,
        AssetTypeId = Guid.NewGuid(),
        DomainId = Guid.NewGuid(),
        UseCaseId = Guid.NewGuid(),
        ToolIds = toolIds ?? [Guid.NewGuid()],
        Tags = tags ?? ["react", "typescript"]
    };

    [Fact]
    public async Task UpdateAssetValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new UpdateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidUpdateCommand(), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAssetValidator_ShouldThrow_WhenAssetIdIsEmpty()
    {
        var validator = new UpdateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidUpdateCommand(assetId: Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAssetValidator_ShouldThrow_WhenRequestingUserIdIsEmpty()
    {
        var validator = new UpdateAssetCommandValidator();
        var act = () => validator.ValidateAsync(ValidUpdateCommand(requestingUserId: Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAssetValidator_ShouldReuseCreateRules_WhenToolIdsContainDuplicates()
    {
        var validator = new UpdateAssetCommandValidator();
        var toolId = Guid.NewGuid();
        var act = () => validator.ValidateAsync(ValidUpdateCommand(toolIds: [toolId, toolId]), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    // -- DeleteAssetCommandValidator -------------------------------------------

    [Fact]
    public async Task DeleteAssetValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new DeleteAssetCommandValidator();
        var command = new DeleteAssetCommand { AssetId = Guid.NewGuid(), RequestingUserId = Guid.NewGuid() };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAssetValidator_ShouldThrow_WhenAssetIdIsEmpty()
    {
        var validator = new DeleteAssetCommandValidator();
        var command = new DeleteAssetCommand { AssetId = Guid.Empty, RequestingUserId = Guid.NewGuid() };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAssetValidator_ShouldThrow_WhenRequestingUserIdIsEmpty()
    {
        var validator = new DeleteAssetCommandValidator();
        var command = new DeleteAssetCommand { AssetId = Guid.NewGuid(), RequestingUserId = Guid.Empty };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    // -- UploadMarkdownFileCommandValidator ------------------------------------

    [Theory]
    [InlineData("file.md")]
    [InlineData("FILE.MD")]
    [InlineData("document.Md")]
    public async Task UploadValidator_ShouldPass_WhenFileIsValidMarkdown(string fileName)
    {
        var validator = new UploadMarkdownFileCommandValidator();
        var command = new UploadMarkdownFileCommand
        {
            FileContent = "# Hello"u8.ToArray(),
            FileName = fileName
        };
        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("file.txt")]
    [InlineData("file.docx")]
    [InlineData("file")]
    public async Task UploadValidator_ShouldThrow_WhenExtensionIsNotMarkdown(string fileName)
    {
        var validator = new UploadMarkdownFileCommandValidator();
        var command = new UploadMarkdownFileCommand
        {
            FileContent = "# Hello"u8.ToArray(),
            FileName = fileName
        };
        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UploadValidator_ShouldThrow_WhenFileIsEmpty()
    {
        var validator = new UploadMarkdownFileCommandValidator();
        var command = new UploadMarkdownFileCommand { FileContent = [], FileName = "file.md" };
        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UploadValidator_ShouldThrow_WhenFileSizeExceedsLimit()
    {
        var validator = new UploadMarkdownFileCommandValidator();
        var command = new UploadMarkdownFileCommand
        {
            FileContent = new byte[5 * 1024 * 1024 + 1],
            FileName = "file.md"
        };
        var act = () => validator.ValidateAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
