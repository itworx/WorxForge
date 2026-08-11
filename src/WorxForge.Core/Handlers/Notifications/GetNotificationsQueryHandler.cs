using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Queries.Notifications;

namespace WorxForge.Core.Handlers.Notifications;

public sealed class GetNotificationsQueryHandler(INotificationService notificationService) : IQueryHandler<GetNotificationsQuery, PagedResultDto<NotificationDto>>
{
    public Task<PagedResultDto<NotificationDto>> HandleAsync(GetNotificationsQuery query, CancellationToken cancellationToken) =>
        notificationService.GetNotificationsAsync(query.UserId, query.IsRead, query.Page, query.PageSize, cancellationToken);
}
