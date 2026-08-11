using Microsoft.Extensions.Logging;
using WorxForge.Core.Interfaces.Notifications;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;
using WorxForge.Domain.Errors;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Infrastructure.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    INotificationQueue notificationQueue,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(Guid userId, bool? isRead, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, total) = await notificationRepository.GetByUserAsync(userId, isRead, page, pageSize, cancellationToken);
        return new PagedResultDto<NotificationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken) =>
        notificationRepository.GetUnreadCountAsync(userId, cancellationToken);

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new NotFoundException(Errors.NotificationNotFound);

        if (notification.RecipientUserId != userId)
        {
            throw new ForbiddenException(Errors.NotificationForbidden);
        }

        if (notification.IsRead)
        {
            throw new ConflictException(Errors.NotificationAlreadyRead);
        }

        await notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
    }

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken) =>
        notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);

    public async Task SendAsync(Guid recipientUserId, Guid? actorUserId, NotificationType type, string message, Guid? assetId, Guid? commentId, CancellationToken cancellationToken)
    {
        if (recipientUserId == actorUserId)
        {
            return;
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            ActorUserId = actorUserId,
            Type = type.ToString(),
            Message = message,
            AssetId = assetId,
            CommentId = commentId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // Persist first so the notification survives even if delivery never happens.
        await notificationRepository.AddAsync(notification, cancellationToken);

        try
        {
            notificationQueue.QueueNotification(new NotificationDeliveryMessage
            {
                NotificationId = notification.NotificationId,
                RecipientUserId = notification.RecipientUserId,
                ActorUserId = notification.ActorUserId,
                Type = type,
                Message = notification.Message,
                AssetId = notification.AssetId,
                CommentId = notification.CommentId,
                CreatedAt = notification.CreatedAt
            });
        }
        catch (Exception exception)
        {
            // Delivery is best-effort; the persisted row is the source of truth if this fails.
            logger.LogError(exception, "Failed to enqueue notification {NotificationId} for delivery.", notification.NotificationId);
        }
    }
}
