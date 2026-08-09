using WorxForge.Core.Abstractions;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Commands.Profile;
using WorxForge.Domain.Errors;
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
        group.MapGet("/{userId:guid}/assets", GetUserAssetsAsync);
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
            throw new UnauthorizedException(Errors.AuthenticationInvalidCredentials);
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
            throw new UnauthorizedException(Errors.AuthenticationInvalidCredentials);
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

    private static async Task<IResult> GetUserAssetsAsync(
        Guid userId,
        int? page,
        int? pageSize,
        IQueryHandler<GetUserAssetsQuery, PagedResultDto<MyAssetDto>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetUserAssetsQuery
        {
            TargetUserId = userId,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        };

        var response = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(response);
    }
}
