using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<NotificationDto> Items, int Total)> GetByUserAsync(Guid userId, bool? isRead, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
}

