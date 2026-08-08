using System.Security.Claims;
using WorxForge.API.Models.Requests.Notifications;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Notifications;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Notifications;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Notifications;

namespace WorxForge.API.EndpointGroups;

public static class NotificationsEndpointGroup
{
    public static RouteGroupBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapGet("/", GetNotificationsAsync);
        group.MapGet("/unread-count", GetUnreadNotificationCountAsync);
        group.MapPut("/{notificationId:guid}/read", MarkNotificationAsReadAsync);
        group.MapPut("/read-all", MarkAllNotificationsAsReadAsync);
        return group;
    }

    private static async Task<IResult> GetNotificationsAsync(
        [AsParameters] NotificationFilters filters,
        ClaimsPrincipal user,
        IRequestValidator<GetNotificationsQuery> validator,
        IQueryHandler<GetNotificationsQuery, PagedResultDto<NotificationDto>> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimConstants.UserId), out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetNotificationsQuery
        {
            UserId = userId,
            IsRead = filters.IsRead,
            Page = filters.Page ?? 1,
            PageSize = filters.PageSize ?? 20
        };

        try
        {
            await validator.ValidateAsync(query, cancellationToken);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
    }

    private static async Task<IResult> GetUnreadNotificationCountAsync(
        ClaimsPrincipal user,
        IRequestValidator<GetUnreadNotificationCountQuery> validator,
        IQueryHandler<GetUnreadNotificationCountQuery, UnreadNotificationCountDto> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimConstants.UserId), out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetUnreadNotificationCountQuery { UserId = userId };

        try
        {
            await validator.ValidateAsync(query, cancellationToken);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
    }

    private static async Task<IResult> MarkNotificationAsReadAsync(
        Guid notificationId,
        ClaimsPrincipal user,
        IRequestValidator<MarkNotificationAsReadCommand> validator,
        ICommandHandler<MarkNotificationAsReadCommand> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimConstants.UserId), out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new MarkNotificationAsReadCommand
        {
            UserId = userId,
            NotificationId = notificationId
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            await handler.HandleAsync(command, cancellationToken);
            return Results.NoContent();
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
    }

    private static async Task<IResult> MarkAllNotificationsAsReadAsync(
        ClaimsPrincipal user,
        IRequestValidator<MarkAllNotificationsAsReadCommand> validator,
        ICommandHandler<MarkAllNotificationsAsReadCommand> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimConstants.UserId), out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new MarkAllNotificationsAsReadCommand { UserId = userId };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            await handler.HandleAsync(command, cancellationToken);
            return Results.NoContent();
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
    }
}
