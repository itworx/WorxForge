using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Enums;

namespace WorxForge.Core.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(Guid userId, bool? isRead, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task SendAsync(Guid recipientUserId, Guid? actorUserId, NotificationType type, string message, Guid? assetId, Guid? commentId, CancellationToken cancellationToken);
}

