using Microsoft.AspNetCore.SignalR;
using WorxForge.API.Hubs;
using WorxForge.Core.Interfaces.Notifications;

namespace WorxForge.API.Services;

// Only place SignalR delivery logic lives: consumes the in-process queue and pushes to connected
// clients. Delivery failures are logged and swallowed so they never affect the request that
// enqueued the notification (which already persisted successfully before this runs).
public sealed class NotificationDeliveryBackgroundService(
    INotificationQueue notificationQueue,
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationDeliveryBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in notificationQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await hubContext.Clients
                    .User(message.RecipientUserId.ToString())
                    .SendAsync("ReceiveNotification", message, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to deliver notification {NotificationId} to user {RecipientUserId}.",
                    message.NotificationId, message.RecipientUserId);
            }
        }
    }
}
