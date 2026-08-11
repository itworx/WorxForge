using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Notifications;

namespace WorxForge.Core.Handlers.Notifications;

public sealed class MarkNotificationAsReadCommandHandler(INotificationService notificationService) : ICommandHandler<MarkNotificationAsReadCommand>
{
    public Task HandleAsync(MarkNotificationAsReadCommand command, CancellationToken cancellationToken) =>
        notificationService.MarkAsReadAsync(command.UserId, command.NotificationId, cancellationToken);
}
