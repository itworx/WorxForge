using System.Security.Claims;
using WorxForge.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Commands.Library;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;
using WorxForge.API.Models.Requests.Comments;
using WorxForge.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace WorxForge.API.EndpointGroups;

public static class AssetsEndpointGroup
{
    public static RouteGroupBuilder MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets");
        group.MapPost("/", CreateAssetAsync);
        group.MapPut("/{assetId:guid}", UpdateAssetAsync);
        group.MapDelete("/{assetId:guid}", DeleteAssetAsync);
        group.MapPost("/preview", PreviewAssetAsync);
        group.MapGet("/{assetId:guid}/comments", GetAssetCommentsAsync);
        group.MapPost("/{assetId:guid}/comments", CreateAssetCommentAsync);
        group.MapDelete("/{assetId:guid}/comments/{commentId:guid}", DeleteAssetCommentAsync);
        group.MapPost("/{assetId:guid}/bookmark", BookmarkAssetAsync);
        group.MapDelete("/{assetId:guid}/bookmark", RemoveBookmarkAsync);
        group.MapPost("/{assetId:guid}/reaction", ReactToAssetAsync);
        group.MapGet("/{assetId:guid}", GetAssetDetailsAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        return group;
    }

    private static Task<IResult> CreateAssetAsync(CreateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> UpdateAssetAsync(UpdateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetAsync([FromBody] DeleteAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> PreviewAssetAsync(PreviewAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static async Task<IResult> GetAssetDetailsAsync(
        Guid assetId,
        ClaimsPrincipal user,
        IRequestValidator<GetAssetDetailsQuery> validator,
        IQueryHandler<GetAssetDetailsQuery, AssetDetailsDto> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetAssetDetailsQuery
        {
            AssetId = assetId,
            RequestingUserId = userId
        };

        try
        {
            await validator.ValidateAsync(query, cancellationToken);
            var details = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(details);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
    }
    private static Task<IResult> GetAssetCommentsAsync([AsParameters] GetAssetCommentsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> CreateAssetCommentAsync(CreateAssetCommentCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetCommentAsync([FromBody] DeleteAssetCommentCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> BookmarkAssetAsync(BookmarkAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> RemoveBookmarkAsync([FromBody] RemoveBookmarkCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> ReactToAssetAsync(ReactToAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static async Task<IResult> ReactToAssetAsync(
        Guid assetId,
        ReactToAssetRequest request,
        ClaimsPrincipal user,
        IRequestValidator<ReactToAssetCommand> validator,
        ICommandHandler<ReactToAssetCommand, AssetReactionDto> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new ReactToAssetCommand
        {
            UserId = userId,
            AssetId = assetId,
            ReactionType = request.ReactionType
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            var response = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(response);
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static async Task<IResult> RemoveAssetReactionAsync(
        Guid assetId,
        ClaimsPrincipal user,
        IRequestValidator<RemoveAssetReactionCommand> validator,
        ICommandHandler<RemoveAssetReactionCommand, AssetReactionDto> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new RemoveAssetReactionCommand
        {
            UserId = userId,
            AssetId = assetId
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            var response = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(response);
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
    }
}
