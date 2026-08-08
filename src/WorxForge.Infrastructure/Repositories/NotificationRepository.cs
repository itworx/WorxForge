using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class NotificationRepository(WorxForgeDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken) =>
        dbContext.Notifications.SingleOrDefaultAsync(notification => notification.NotificationId == notificationId, cancellationToken);

    public async Task<(IReadOnlyList<NotificationDto> Items, int Total)> GetByUserAsync(Guid userId, bool? isRead, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(notification => notification.IsRead == isRead.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(notification => new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Type = ParseNotificationType(notification.Type),
                Message = notification.Message,
                AssetId = notification.AssetId,
                CommentId = notification.CommentId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    private static NotificationType ParseNotificationType(string type) =>
        Enum.TryParse<NotificationType>(type, out var parsed) ? parsed : NotificationType.Unknown;

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Notifications.CountAsync(notification => notification.RecipientUserId == userId && !notification.IsRead, cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await dbContext.Notifications.AddAsync(notification, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .SingleOrDefaultAsync(existing => existing.NotificationId == notificationId, cancellationToken);

        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var readAt = DateTime.UtcNow;
        await dbContext.Notifications
            .Where(notification => notification.RecipientUserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(notification => notification.IsRead, true)
                .SetProperty(notification => notification.ReadAt, readAt), cancellationToken);
    }
}
