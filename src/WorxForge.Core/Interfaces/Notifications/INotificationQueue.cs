using WorxForge.Domain.Dtos.Notifications;

namespace WorxForge.Core.Interfaces.Notifications;

/// <summary>
/// In-process producer/consumer queue (backed by System.Threading.Channels) that decouples
/// notification persistence (request path) from real-time delivery (background service).
/// </summary>
public interface INotificationQueue
{
    void QueueNotification(NotificationDeliveryMessage message);

    IAsyncEnumerable<NotificationDeliveryMessage> DequeueAllAsync(CancellationToken cancellationToken);
}
