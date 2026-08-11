using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Notifications;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Notifications;

public sealed class MarkAllNotificationsAsReadCommandValidator : IRequestValidator<MarkAllNotificationsAsReadCommand>
{
    public Task ValidateAsync(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.UserId)] = ["UserId is required."]
            });
        }

        return Task.CompletedTask;
    }
}
