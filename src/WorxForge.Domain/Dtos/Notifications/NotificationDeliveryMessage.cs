using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Dtos.Notifications;

// Payload queued in-process (System.Threading.Channels) for background SignalR delivery, after persistence.
public sealed class NotificationDeliveryMessage
{
    public Guid NotificationId { get; init; }
    public Guid RecipientUserId { get; init; }
    public Guid? ActorUserId { get; init; }
    public NotificationType Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AssetId { get; init; }
    public Guid? CommentId { get; init; }
    public DateTime CreatedAt { get; init; }
}
