using FluentAssertions;
using WorxForge.Core.Validators.Assets;
using WorxForge.Core.Validators.Comments;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.UnitTests;

public sealed class CommentsValidatorsTests
{
    private static CreateAssetCommentCommand ValidCreateCommand(string content = "Great discussion", Guid? parentCommentId = null) => new()
    {
        UserId = Guid.NewGuid(),
        AssetId = Guid.NewGuid(),
        ParentCommentId = parentCommentId,
        Content = content
    };

    [Fact]
    public async Task CreateCommentValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new CreateAssetCommentCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCommentValidator_ShouldThrow_WhenContentIsEmpty(string content)
    {
        var validator = new CreateAssetCommentCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(content: content), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateCommentValidator_ShouldThrow_WhenReplyCommentIdIsEmpty()
    {
        var validator = new CreateAssetCommentCommandValidator();
        var command = ValidCreateCommand(parentCommentId: Guid.Empty);

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateCommentValidator_ShouldThrow_WhenContentExceedsLimit()
    {
        var validator = new CreateAssetCommentCommandValidator();
        var act = () => validator.ValidateAsync(ValidCreateCommand(content: new string('x', 2001)), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateCommentValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new UpdateAssetCommentCommandValidator();
        var command = new UpdateAssetCommentCommand
        {
            AssetId = Guid.NewGuid(),
            CommentId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid(),
            Content = "Updated content"
        };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateCommentValidator_ShouldThrow_WhenContentIsEmpty(string content)
    {
        var validator = new UpdateAssetCommentCommandValidator();
        var command = new UpdateAssetCommentCommand
        {
            AssetId = Guid.NewGuid(),
            CommentId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid(),
            Content = content
        };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteCommentValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new DeleteAssetCommentCommandValidator();
        var command = new DeleteAssetCommentCommand
        {
            AssetId = Guid.NewGuid(),
            CommentId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid()
        };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAssetCommentsValidator_ShouldPass_WhenQueryIsValid()
    {
        var validator = new GetAssetCommentsQueryValidator();
        var query = new GetAssetCommentsQuery
        {
            AssetId = Guid.NewGuid(),
            Page = 1,
            PageSize = 20
        };

        var act = () => validator.ValidateAsync(query, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAssetCommentsValidator_ShouldThrow_WhenPageIsInvalid()
    {
        var validator = new GetAssetCommentsQueryValidator();
        var query = new GetAssetCommentsQuery
        {
            AssetId = Guid.NewGuid(),
            Page = 0,
            PageSize = 20
        };

        var act = () => validator.ValidateAsync(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAssetCommentThreadValidator_ShouldPass_WhenQueryIsValid()
    {
        var validator = new GetAssetCommentThreadQueryValidator();
        var query = new GetAssetCommentThreadQuery
        {
            AssetId = Guid.NewGuid(),
            CommentId = Guid.NewGuid()
        };

        var act = () => validator.ValidateAsync(query, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}