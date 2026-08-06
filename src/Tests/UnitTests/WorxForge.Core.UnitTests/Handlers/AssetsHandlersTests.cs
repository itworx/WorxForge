using FluentAssertions;
using Moq;
using WorxForge.Core.Abstractions;
using WorxForge.Core.Handlers.Assets;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.UnitTests;

public sealed class AssetsHandlersTests
{
    // -- UploadMarkdownFileCommandHandler --------------------------------------

    [Fact]
    public async Task UploadMarkdownHandler_ShouldReturn_MarkdownContent_WhenFileIsValid()
    {
        var validator = new Mock<IRequestValidator<UploadMarkdownFileCommand>>();
        var handler = new UploadMarkdownFileCommandHandler(validator.Object);
        var content = "# Hello World\n\nSome content.";
        var command = new UploadMarkdownFileCommand
        {
            FileContent = System.Text.Encoding.UTF8.GetBytes(content),
            FileName = "test.md"
        };

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.MarkdownContent.Should().Be(content);
    }

    [Fact]
    public async Task UploadMarkdownHandler_ShouldThrow_WhenFileContainsInvalidUtf8()
    {
        var validator = new Mock<IRequestValidator<UploadMarkdownFileCommand>>();
        var handler = new UploadMarkdownFileCommandHandler(validator.Object);
        // 0xFF is not valid UTF-8.
        var command = new UploadMarkdownFileCommand { FileContent = [0xFF, 0xFE], FileName = "bad.md" };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*UTF-8*");
    }

    [Fact]
    public async Task UploadMarkdownHandler_ShouldThrow_WhenDecodedContentIsEmpty()
    {
        var validator = new Mock<IRequestValidator<UploadMarkdownFileCommand>>();
        var handler = new UploadMarkdownFileCommandHandler(validator.Object);
        var command = new UploadMarkdownFileCommand
        {
            FileContent = System.Text.Encoding.UTF8.GetBytes("   "),
            FileName = "empty.md"
        };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // -- CreateAssetCommandHandler ---------------------------------------------

    [Fact]
    public async Task CreateAssetHandler_ShouldReturn_AssetId_WhenCommandIsValid()
    {
        var validator = new Mock<IRequestValidator<CreateAssetCommand>>();
        var service = new Mock<IAssetService>();
        var expectedId = Guid.NewGuid();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateAssetCommand>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(expectedId);

        var handler = new CreateAssetCommandHandler(validator.Object, service.Object);
        var command = new CreateAssetCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "test-asset",
            Description = "desc",
            MarkdownContent = "# Test",
            AssetTypeId = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            UseCaseId = Guid.NewGuid(),
            ToolIds = [Guid.NewGuid()]
        };

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.AssetId.Should().Be(expectedId);
    }

    [Fact]
    public async Task CreateAssetHandler_ShouldCallValidator_BeforeService()
    {
        var callOrder = new List<string>();
        var validator = new Mock<IRequestValidator<CreateAssetCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateAssetCommand>(), It.IsAny<CancellationToken>()))
                 .Callback(() => callOrder.Add("validator"))
                 .Returns(Task.CompletedTask);

        var service = new Mock<IAssetService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateAssetCommand>(), It.IsAny<CancellationToken>()))
               .Callback(() => callOrder.Add("service"))
               .ReturnsAsync(Guid.NewGuid());

        var handler = new CreateAssetCommandHandler(validator.Object, service.Object);
        await handler.HandleAsync(new CreateAssetCommand(), CancellationToken.None);

        callOrder.Should().ContainInOrder("validator", "service");
    }
}

