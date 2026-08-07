using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Explore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Commands.Library;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;
using WorxForge.Domain.Queries.Lookup;
using WorxForge.API.Models.Requests.Assets;
using WorxForge.API.Models.Requests.Comments;

namespace WorxForge.API.EndpointGroups;

public static class AssetsEndpointGroup
{
    private const int MaxSearchQueryLength = 200;

    public static RouteGroupBuilder MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets");

        group.MapGet("/types", GetAssetTypesAsync);
        group.MapGet("/domains", GetDomainsAsync);
        group.MapGet("/use-cases", GetUseCasesAsync);
        group.MapGet("/tools", GetToolsAsync);

        group.MapPost("/upload-markdown", UploadMarkdownAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser)
            .DisableAntiforgery();

        group.MapPost("/", CreateAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);

        group.MapPut("/{assetId:guid}", UpdateAssetAsync);
        group.MapDelete("/{assetId:guid}", DeleteAssetAsync);
        group.MapPost("/preview", PreviewAssetAsync);
        group.MapGet("/search", SearchAssetsAsync)
            .WithName("SearchAssets")
            .WithSummary("Search knowledge assets")
            .WithDescription("Keyword search over Title/Description plus exact hashtag matching (#tag), combined with the same filters as Explore.")
            .Produces<PagedResultDto<AssetSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        group.MapGet("/{assetId:guid}", GetAssetDetailsAsync);
        group.MapGet("/{assetId:guid}/comments", GetAssetCommentsAsync);
        group.MapPost("/{assetId:guid}/comments", CreateAssetCommentAsync);
        group.MapDelete("/{assetId:guid}/comments/{commentId:guid}", DeleteAssetCommentAsync);
        group.MapPost("/{assetId:guid}/bookmark", BookmarkAssetAsync);
        group.MapDelete("/{assetId:guid}/bookmark", RemoveBookmarkAsync);
        group.MapPost("/{assetId:guid}/reaction", ReactToAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapDelete("/{assetId:guid}/reaction", RemoveAssetReactionAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        return group;
    }

    private static async Task<IResult> GetAssetTypesAsync(
        IQueryHandler<GetAssetTypesQuery, IReadOnlyList<LookupItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetAssetTypesQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetDomainsAsync(
        IQueryHandler<GetDomainsQuery, IReadOnlyList<LookupItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetDomainsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUseCasesAsync(
        IQueryHandler<GetUseCasesQuery, IReadOnlyList<LookupItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetUseCasesQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetToolsAsync(
        IQueryHandler<GetToolsQuery, IReadOnlyList<LookupItemDto>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetToolsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UploadMarkdownAsync(
        IFormFile file,
        ICommandHandler<UploadMarkdownFileCommand, UploadMarkdownResponse> handler,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        var result = await handler.HandleAsync(
            new UploadMarkdownFileCommand { FileContent = bytes, FileName = file.FileName },
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateAssetAsync(
        [FromBody] CreateAssetRequest request,
        HttpContext httpContext,
        ICommandHandler<CreateAssetCommand, PublishAssetResponse> handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;
        if (!Guid.TryParse(userIdClaim, out var authorId))
            throw new WorxForgeException("Authenticated user identifier is invalid.");

        var command = new CreateAssetCommand
        {
            AuthorId = authorId,
            Title = request.Title,
            Description = request.Description,
            MarkdownContent = request.MarkdownContent,
            AssetTypeId = request.AssetTypeId,
            DomainId = request.DomainId,
            UseCaseId = request.UseCaseId,
            ToolIds = request.ToolIds,
            Tags = request.Tags
        };

        var response = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/api/assets/{response.AssetId}", response);
    }

    private static Task<IResult> UpdateAssetAsync(UpdateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetAsync([FromBody] DeleteAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> PreviewAssetAsync(PreviewAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static async Task<IResult> SearchAssetsAsync(
        [AsParameters] AssetFilters filters,
        ClaimsPrincipal user,
        IQueryHandler<SearchAssetsQuery, PagedResultDto<AssetSummaryDto>> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        if (filters.Q is { Length: > MaxSearchQueryLength })
        {
            return Results.BadRequest($"Query must not exceed {MaxSearchQueryLength} characters.");
        }

        var query = new SearchAssetsQuery
        {
            RequestingUserId = userId,
            Q = filters.Q,
            AssetTypeId = filters.AssetTypeId,
            DomainId = filters.DomainId,
            UseCaseId = filters.UseCaseId,
            ToolId = filters.ToolId,
            Freshness = filters.Freshness,
            Page = filters.Page ?? 1,
            PageSize = filters.PageSize ?? 12
        };

        var results = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(results);
    }

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
