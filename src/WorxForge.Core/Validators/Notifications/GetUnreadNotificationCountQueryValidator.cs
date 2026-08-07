using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Notifications;

namespace WorxForge.Core.Validators.Notifications;

public sealed class GetUnreadNotificationCountQueryValidator : IRequestValidator<GetUnreadNotificationCountQuery>
{
    public Task ValidateAsync(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
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
