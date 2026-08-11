using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Notifications;

namespace WorxForge.Core.Handlers.Notifications;

public sealed class MarkAllNotificationsAsReadCommandHandler(INotificationService notificationService) : ICommandHandler<MarkAllNotificationsAsReadCommand>
{
    public Task HandleAsync(MarkAllNotificationsAsReadCommand command, CancellationToken cancellationToken) =>
        notificationService.MarkAllAsReadAsync(command.UserId, cancellationToken);
}
