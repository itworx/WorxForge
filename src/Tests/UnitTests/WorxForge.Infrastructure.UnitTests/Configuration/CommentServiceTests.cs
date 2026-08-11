using FluentAssertions;
using Moq;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;
using WorxForge.Domain.Exceptions;
using WorxForge.Infrastructure.Services;

namespace WorxForge.Infrastructure.UnitTests;

public sealed class CommentServiceTests
{
    private static CommentService BuildService(
        Mock<ICommentRepository>? commentRepo = null,
        Mock<IAssetRepository>? assetRepo = null,
        Mock<IUserRepository>? userRepo = null,
        Mock<INotificationService>? notificationService = null)
    {
        if (assetRepo is null)
        {
            assetRepo = new Mock<IAssetRepository>();
            assetRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid assetId, CancellationToken _) => new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });
        }

        if (commentRepo is null)
        {
            commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Comment?)null);
            commentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            commentRepo.Setup(r => r.UpdateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            commentRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            commentRepo.Setup(r => r.GetThreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetCommentDto?)null);
            commentRepo.Setup(r => r.GetByAssetAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(([], 0));
        }

        if (userRepo is null)
        {
            userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
        }

        if (notificationService is null)
        {
            notificationService = new Mock<INotificationService>();
            notificationService.Setup(s => s.SendAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<NotificationType>(), It.IsAny<string>(),
                    It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return new CommentService(commentRepo.Object, assetRepo.Object, userRepo.Object, notificationService.Object);
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldCreateTopLevelComment()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Comment? capturedComment = null;

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Comment?)null);
        commentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback((Comment comment, CancellationToken _) => capturedComment = comment)
            .Returns(Task.CompletedTask);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);

        var commentId = await service.CreateCommentAsync(userId, assetId, null, "hello world", CancellationToken.None);

        commentId.Should().NotBeEmpty();
        capturedComment.Should().NotBeNull();
        capturedComment!.AssetId.Should().Be(assetId);
        capturedComment.UserId.Should().Be(userId);
        capturedComment.ParentCommentId.Should().BeNull();
        capturedComment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        capturedComment.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldThrow_WhenReplyTargetsReply()
    {
        var assetId = Guid.NewGuid();
        var parentReplyId = Guid.NewGuid();
        var parentTopLevelId = Guid.NewGuid();

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(parentReplyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = parentReplyId,
                AssetId = assetId,
                ParentCommentId = parentTopLevelId,
                UserId = Guid.NewGuid(),
                Content = "reply",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.CreateCommentAsync(Guid.NewGuid(), assetId, parentReplyId, "nested", CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldUpdateContentAndTimestamp()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        Comment? updatedComment = null;

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = ownerId,
                Content = "old",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
            });
        commentRepo.Setup(r => r.UpdateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback((Comment comment, CancellationToken _) => updatedComment = comment)
            .Returns(Task.CompletedTask);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        await service.UpdateCommentAsync(assetId, commentId, ownerId, "new content", CancellationToken.None);

        updatedComment.Should().NotBeNull();
        updatedComment!.Content.Should().Be("new content");
        updatedComment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldThrowForbidden_WhenUserIsNotOwner()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = ownerId,
                Content = "old",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            });

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.UpdateCommentAsync(assetId, commentId, Guid.NewGuid(), "new content", CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldAllowCommentOwner_ToDelete()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var thread = new AssetCommentDto
        {
            CommentId = commentId,
            AssetId = assetId,
            Content = "comment",
            AuthorDisplayName = "Owner",
            CreatedAt = DateTime.UtcNow,
            Replies = []
        };

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = ownerId,
                Content = "comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        commentRepo.Setup(r => r.GetThreadAsync(assetId, commentId, It.IsAny<CancellationToken>())).ReturnsAsync(thread);
        commentRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.DeleteCommentAsync(assetId, commentId, ownerId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        commentRepo.Verify(r => r.SoftDeleteAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(commentId) && ids.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldAllowAssetOwner_ToDelete()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var commenterId = Guid.NewGuid();
        var assetOwnerId = Guid.NewGuid();
        var thread = new AssetCommentDto
        {
            CommentId = commentId,
            AssetId = assetId,
            Content = "comment",
            AuthorDisplayName = "Commenter",
            CreatedAt = DateTime.UtcNow,
            Replies = []
        };

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = commenterId,
                Content = "comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        commentRepo.Setup(r => r.GetThreadAsync(assetId, commentId, It.IsAny<CancellationToken>())).ReturnsAsync(thread);
        commentRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = assetOwnerId, Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.DeleteCommentAsync(assetId, commentId, assetOwnerId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        commentRepo.Verify(r => r.SoftDeleteAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(commentId) && ids.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldSoftDeleteThread_WhenTopLevelCommentIsRemoved()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var replyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var thread = new AssetCommentDto
        {
            CommentId = commentId,
            AssetId = assetId,
            Content = "root",
            AuthorDisplayName = "Author",
            CreatedAt = DateTime.UtcNow,
            Replies = [new AssetCommentDto
            {
                CommentId = replyId,
                AssetId = assetId,
                ParentCommentId = commentId,
                Content = "reply",
                AuthorDisplayName = "Responder",
                CreatedAt = DateTime.UtcNow
            }]
        };

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = ownerId,
                Content = "root",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        commentRepo.Setup(r => r.GetThreadAsync(assetId, commentId, It.IsAny<CancellationToken>())).ReturnsAsync(thread);
        commentRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);

        await service.DeleteCommentAsync(assetId, commentId, ownerId, CancellationToken.None);

        commentRepo.Verify(r => r.SoftDeleteAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(commentId) && ids.Contains(replyId) && ids.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAssetCommentThreadAsync_ShouldThrow_WhenRequestedCommentIsReply()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                ParentCommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "reply",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.GetAssetCommentThreadAsync(assetId, commentId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrowForbidden_WhenUserIsNotOwnerOrAssetOwner()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = Guid.NewGuid(),
                Content = "comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var act = () => service.DeleteCommentAsync(assetId, commentId, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetAssetCommentThreadAsync_ShouldReturnThread()
    {
        var assetId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var thread = new AssetCommentDto
        {
            CommentId = commentId,
            AssetId = assetId,
            Content = "root",
            AuthorDisplayName = "Author",
            CreatedAt = DateTime.UtcNow,
            Replies = []
        };

        var commentRepo = new Mock<ICommentRepository>();
        commentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment
            {
                CommentId = commentId,
                AssetId = assetId,
                UserId = Guid.NewGuid(),
                Content = "root",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        commentRepo.Setup(r => r.GetThreadAsync(assetId, commentId, It.IsAny<CancellationToken>())).ReturnsAsync(thread);

        var assetRepo = new Mock<IAssetRepository>();
        assetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Asset { AssetId = assetId, AuthorId = Guid.NewGuid(), Title = "asset", MarkdownContent = "md" });

        var service = BuildService(commentRepo: commentRepo, assetRepo: assetRepo);
        var result = await service.GetAssetCommentThreadAsync(assetId, commentId, CancellationToken.None);

        result.Should().BeEquivalentTo(thread);
    }
}