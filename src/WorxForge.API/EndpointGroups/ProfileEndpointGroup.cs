using WorxForge.Core.Abstractions;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Commands.Profile;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.API.EndpointGroups;

public static class ProfileEndpointGroup
{
    public static RouteGroupBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile");
        group.MapGet("/me", GetMyProfileAsync);
        group.MapGet("/{userId:guid}", GetPublicProfileAsync);
        group.MapPut("/me", UpdateMyProfileAsync);
        return group;
    }

    private static async Task<IResult> GetMyProfileAsync(
        HttpContext httpContext,
        IQueryHandler<GetMyProfileQuery, ProfileDto> handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new WorxForgeException("Authenticated user identifier is invalid.");
        }

        var response = await handler.HandleAsync(new GetMyProfileQuery { UserId = userId }, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetPublicProfileAsync(
        Guid userId,
        IQueryHandler<GetPublicProfileQuery, ProfileDto> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new GetPublicProfileQuery { TargetUserId = userId }, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateMyProfileAsync(
        HttpContext httpContext,
        UpdateMyProfileCommand command,
        ICommandHandler<UpdateMyProfileCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new WorxForgeException("Authenticated user identifier is invalid.");
        }

        await handler.HandleAsync(
            new UpdateMyProfileCommand
            {
                UserId = userId,
                DisplayName = command.DisplayName,
                Username = command.Username,
                AvatarUrl = command.AvatarUrl
            },
            cancellationToken);

        return Results.NoContent();
    }
}
