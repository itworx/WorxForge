using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Notifications;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Notifications;

public sealed class MarkNotificationAsReadCommandValidator : IRequestValidator<MarkNotificationAsReadCommand>
{
    public Task ValidateAsync(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        Dictionary<string, string[]> errors = [];

        if (request.UserId == Guid.Empty)
        {
            errors[nameof(request.UserId)] = ["UserId is required."];
        }

        if (request.NotificationId == Guid.Empty)
        {
            errors[nameof(request.NotificationId)] = ["NotificationId is required."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return Task.CompletedTask;
    }
}
