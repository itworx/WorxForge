using System.Threading.Channels;
using WorxForge.Core.Interfaces.Notifications;
using WorxForge.Domain.Dtos.Notifications;

namespace WorxForge.Infrastructure.Services;

// Singleton in-process queue: producers (request path) never touch SignalR; only the background
// consumer (API layer) reads from it and pushes over the hub.
public sealed class NotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationDeliveryMessage> _channel = Channel.CreateUnbounded<NotificationDeliveryMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void QueueNotification(NotificationDeliveryMessage message) =>
        _channel.Writer.TryWrite(message);

    public IAsyncEnumerable<NotificationDeliveryMessage> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
