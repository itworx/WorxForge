using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class CommentRepository(WorxForgeDbContext dbContext) : ICommentRepository
{
    private sealed record CommentRow(
        Guid CommentId,
        Guid AssetId,
        Guid? ParentCommentId,
        string AuthorDisplayName,
        string? AuthorAvatarUrl,
        string Content,
        DateTime CreatedAt);

    public Task<Comment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken) =>
        dbContext.Comments.AsNoTracking().SingleOrDefaultAsync(comment => comment.CommentId == commentId && comment.DeletedAt == null, cancellationToken);

    public async Task<(IReadOnlyList<AssetCommentDto> Items, int Total)> GetByAssetAsync(Guid assetId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var rows = await LoadRowsAsync(dbContext.Comments.AsNoTracking().Where(comment => comment.AssetId == assetId && comment.DeletedAt == null), cancellationToken);
        return BuildPagedThread(rows, page, pageSize);
    }

    public async Task<(IReadOnlyList<AssetCommentDto> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var rows = await LoadRowsAsync(dbContext.Comments.AsNoTracking().Where(comment => comment.DeletedAt == null), cancellationToken);
        return BuildPagedThread(rows, page, pageSize);
    }

    public async Task<AssetCommentDto?> GetThreadAsync(Guid assetId, Guid commentId, CancellationToken cancellationToken)
    {
        var requestedComment = await dbContext.Comments
            .AsNoTracking()
            .SingleOrDefaultAsync(comment => comment.CommentId == commentId && comment.AssetId == assetId && comment.DeletedAt == null, cancellationToken);

        if (requestedComment is null)
        {
            return null;
        }

        var rootCommentId = requestedComment.ParentCommentId ?? requestedComment.CommentId;
        var rows = await LoadRowsAsync(
            dbContext.Comments.AsNoTracking().Where(comment =>
                comment.AssetId == assetId
                && comment.DeletedAt == null
                && (comment.CommentId == rootCommentId || comment.ParentCommentId == rootCommentId)),
            cancellationToken);

        var rootRow = rows.SingleOrDefault(row => row.CommentId == rootCommentId && row.ParentCommentId is null);
        if (rootRow is null)
        {
            return null;
        }

        var repliesByParent = rows
            .Where(row => row.ParentCommentId.HasValue)
            .GroupBy(row => row.ParentCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(row => row.CreatedAt).ThenBy(row => row.CommentId).ToList());

        return MapComment(rootRow, repliesByParent);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken)
    {
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken)
    {
        dbContext.Comments.Update(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(IReadOnlyCollection<Guid> commentIds, CancellationToken cancellationToken)
    {
        if (commentIds.Count == 0)
        {
            return;
        }

        var comments = await dbContext.Comments
            .Where(comment => commentIds.Contains(comment.CommentId) && comment.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (comments.Count == 0)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var comment in comments)
        {
            comment.DeletedAt = nowUtc;
            comment.UpdatedAt = nowUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<CommentRow>> LoadRowsAsync(IQueryable<Comment> comments, CancellationToken cancellationToken) =>
        await comments
            .Join(dbContext.Users.AsNoTracking(), comment => comment.UserId, user => user.UserId, (comment, user) => new CommentRow(
                comment.CommentId,
                comment.AssetId,
                comment.ParentCommentId,
                user.DisplayName,
                user.AvatarUrl,
                comment.Content,
                comment.CreatedAt))
            .OrderBy(row => row.CreatedAt)
            .ThenBy(row => row.CommentId)
            .ToListAsync(cancellationToken);

    private static (IReadOnlyList<AssetCommentDto> Items, int Total) BuildPagedThread(IReadOnlyList<CommentRow> rows, int page, int pageSize)
    {
        var topLevelRows = rows.Where(row => row.ParentCommentId is null).OrderBy(row => row.CreatedAt).ThenBy(row => row.CommentId).ToList();
        var repliesByParent = rows
            .Where(row => row.ParentCommentId.HasValue)
            .GroupBy(row => row.ParentCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(row => row.CreatedAt).ThenBy(row => row.CommentId).ToList());

        var items = topLevelRows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => MapComment(row, repliesByParent))
            .ToList();

        return (items, topLevelRows.Count);
    }

    private static AssetCommentDto MapComment(CommentRow row, IReadOnlyDictionary<Guid, List<CommentRow>> repliesByParent) =>
        new()
        {
            CommentId = row.CommentId,
            AssetId = row.AssetId,
            ParentCommentId = row.ParentCommentId,
            AuthorDisplayName = row.AuthorDisplayName,
            AuthorAvatarUrl = row.AuthorAvatarUrl,
            Content = row.Content,
            CreatedAt = row.CreatedAt,
            Replies = repliesByParent.TryGetValue(row.CommentId, out var replies)
                ? replies.Select(reply => MapComment(reply, repliesByParent)).ToList()
                : []
        };
}
