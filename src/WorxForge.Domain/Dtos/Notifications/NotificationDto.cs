using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Dtos.Notifications;

public sealed class NotificationDto
{
    public Guid NotificationId { get; init; }
    public NotificationType Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AssetId { get; init; }
    public Guid? CommentId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

