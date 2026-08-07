using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Queries.Notifications;

namespace WorxForge.Core.Handlers.Notifications;

public sealed class GetUnreadNotificationCountQueryHandler(INotificationService notificationService) : IQueryHandler<GetUnreadNotificationCountQuery, UnreadNotificationCountDto>
{
    public async Task<UnreadNotificationCountDto> HandleAsync(GetUnreadNotificationCountQuery query, CancellationToken cancellationToken)
    {
        var count = await notificationService.GetUnreadCountAsync(query.UserId, cancellationToken);
        return new UnreadNotificationCountDto { Count = count };
    }
}
