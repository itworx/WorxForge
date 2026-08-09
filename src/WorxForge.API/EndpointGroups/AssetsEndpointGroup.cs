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
using WorxForge.Domain.Commands.Reports;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Errors;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;
using WorxForge.Domain.Queries.Lookup;
using WorxForge.API.Models.Requests.Assets;
using WorxForge.API.Models.Requests.Comments;
using WorxForge.API.Models.Requests.Reports;

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

        group.MapPut("/{assetId:guid}", UpdateAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapDelete("/{assetId:guid}", DeleteAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/preview", PreviewAssetAsync);
        group.MapGet("/search", SearchAssetsAsync)
            .WithName("SearchAssets")
            .WithSummary("Search knowledge assets")
            .WithDescription("Keyword search over Title/Description plus exact hashtag matching (#tag), combined with the same filters as Explore.")
            .Produces<PagedResultDto<AssetSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        group.MapGet("/{assetId:guid}", GetAssetDetailsAsync);
        group.MapGet("/{assetId:guid}/comments", GetAssetCommentsAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapGet("/{assetId:guid}/comments/{commentId:guid}", GetAssetCommentThreadAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/{assetId:guid}/comments", CreateAssetCommentAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/{assetId:guid}/comments/{commentId:guid}/replies", CreateAssetCommentReplyAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPut("/{assetId:guid}/comments/{commentId:guid}", UpdateAssetCommentAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapDelete("/{assetId:guid}/comments/{commentId:guid}", DeleteAssetCommentAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/{assetId:guid}/bookmark", BookmarkAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapDelete("/{assetId:guid}/bookmark", RemoveBookmarkAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/{assetId:guid}/reaction", ReactToAssetAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapDelete("/{assetId:guid}/reaction", RemoveAssetReactionAsync)
            .RequireAuthorization(PolicyConstants.AuthenticatedUser);
        group.MapPost("/{assetId:guid}/report", ReportAssetAsync)
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
            throw new UnauthorizedException(Errors.AuthenticationInvalidCredentials);

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

    private static async Task<IResult> UpdateAssetAsync(
        Guid assetId,
        [FromBody] UpdateAssetRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateAssetCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;
        if (!Guid.TryParse(userIdClaim, out var requestingUserId))
            throw new UnauthorizedException(Errors.AuthenticationInvalidCredentials);

        var command = new UpdateAssetCommand
        {
            AssetId = assetId,
            RequestingUserId = requestingUserId,
            Title = request.Title,
            Description = request.Description,
            MarkdownContent = request.MarkdownContent,
            AssetTypeId = request.AssetTypeId,
            DomainId = request.DomainId,
            UseCaseId = request.UseCaseId,
            ToolIds = request.ToolIds,
            Tags = request.Tags
        };

        await handler.HandleAsync(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteAssetAsync(
        Guid assetId,
        HttpContext httpContext,
        ICommandHandler<DeleteAssetCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimConstants.UserId)?.Value;
        if (!Guid.TryParse(userIdClaim, out var requestingUserId))
            throw new UnauthorizedException(Errors.AuthenticationInvalidCredentials);

        var command = new DeleteAssetCommand
        {
            AssetId = assetId,
            RequestingUserId = requestingUserId
        };

        await handler.HandleAsync(command, cancellationToken);
        return Results.NoContent();
    }
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
            PageSize = filters.PageSize ?? 20
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

        await validator.ValidateAsync(query, cancellationToken);
        var details = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(details);
    }

    private static async Task<IResult> GetAssetCommentsAsync(
        [AsParameters] GetAssetCommentsQuery query,
        IQueryHandler<GetAssetCommentsQuery, PagedResultDto<AssetCommentDto>> handler,
        CancellationToken cancellationToken)
    {
        var results = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(results);
    }

    private static async Task<IResult> GetAssetCommentThreadAsync(
        Guid assetId,
        Guid commentId,
        IQueryHandler<GetAssetCommentThreadQuery, AssetCommentDto> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAssetCommentThreadQuery
        {
            AssetId = assetId,
            CommentId = commentId
        };

        var result = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateAssetCommentAsync(
        Guid assetId,
        [FromBody] CreateAssetCommentRequest request,
        ClaimsPrincipal user,
        ICommandHandler<CreateAssetCommentCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateAssetCommentCommand
        {
            UserId = userId,
            AssetId = assetId,
            Content = request.Content
        };

        var commentId = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/api/assets/{assetId}/comments/{commentId}", commentId);
    }

    private static async Task<IResult> CreateAssetCommentReplyAsync(
        Guid assetId,
        Guid commentId,
        [FromBody] CreateAssetCommentRequest request,
        ClaimsPrincipal user,
        ICommandHandler<CreateAssetCommentCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateAssetCommentCommand
        {
            UserId = userId,
            AssetId = assetId,
            ParentCommentId = commentId,
            Content = request.Content
        };

        var createdCommentId = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/api/assets/{assetId}/comments/{commentId}", createdCommentId);
    }

    private static async Task<IResult> UpdateAssetCommentAsync(
        Guid assetId,
        Guid commentId,
        [FromBody] CreateAssetCommentRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UpdateAssetCommentCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new UpdateAssetCommentCommand
        {
            AssetId = assetId,
            CommentId = commentId,
            RequestingUserId = userId,
            Content = request.Content
        };

        await handler.HandleAsync(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteAssetCommentAsync(
        Guid assetId,
        Guid commentId,
        ClaimsPrincipal user,
        ICommandHandler<DeleteAssetCommentCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new DeleteAssetCommentCommand
        {
            AssetId = assetId,
            CommentId = commentId,
            RequestingUserId = userId
        };

        await handler.HandleAsync(command, cancellationToken);
        return Results.NoContent();
    }
    private static async Task<IResult> BookmarkAssetAsync(
        Guid assetId,
        ClaimsPrincipal user,
        IRequestValidator<BookmarkAssetCommand> validator,
        ICommandHandler<BookmarkAssetCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new BookmarkAssetCommand
        {
            UserId = userId,
            AssetId = assetId
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            await handler.HandleAsync(command, cancellationToken);
            return Results.Ok();
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

    private static async Task<IResult> RemoveBookmarkAsync(
        Guid assetId,
        ClaimsPrincipal user,
        IRequestValidator<RemoveBookmarkCommand> validator,
        ICommandHandler<RemoveBookmarkCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new RemoveBookmarkCommand
        {
            UserId = userId,
            AssetId = assetId
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            await handler.HandleAsync(command, cancellationToken);
            return Results.Ok();
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

        await validator.ValidateAsync(command, cancellationToken);
        var response = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(response);
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

        await validator.ValidateAsync(command, cancellationToken);
        var response = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> ReportAssetAsync(
        Guid assetId,
        [FromBody] ReportRequest request,
        ClaimsPrincipal user,
        IRequestValidator<ReportAssetCommand> validator,
        ICommandHandler<ReportAssetCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new ReportAssetCommand
        {
            UserId = userId,
            AssetId = assetId,
            Reason = request.Reason
        };

        try
        {
            await validator.ValidateAsync(command, cancellationToken);
            await handler.HandleAsync(command, cancellationToken);
            return Results.Ok();
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
