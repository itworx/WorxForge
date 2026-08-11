namespace WorxForge.Domain.Commands.Notifications;

public sealed class MarkNotificationAsReadCommand
{
    public Guid UserId { get; init; }
    public Guid NotificationId { get; init; }
}

