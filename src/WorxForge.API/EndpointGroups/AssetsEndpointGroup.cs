using Microsoft.AspNetCore.Mvc;
using WorxForge.Core.Abstractions;
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

namespace WorxForge.API.EndpointGroups;

public static class AssetsEndpointGroup
{
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
        group.MapGet("/{assetId:guid}", GetAssetDetailsAsync);
        group.MapGet("/{assetId:guid}/comments", GetAssetCommentsAsync);
        group.MapPost("/{assetId:guid}/comments", CreateAssetCommentAsync);
        group.MapDelete("/{assetId:guid}/comments/{commentId:guid}", DeleteAssetCommentAsync);
        group.MapPost("/{assetId:guid}/bookmark", BookmarkAssetAsync);
        group.MapDelete("/{assetId:guid}/bookmark", RemoveBookmarkAsync);
        group.MapPost("/{assetId:guid}/follow", FollowAssetAsync);
        group.MapDelete("/{assetId:guid}/follow", UnfollowAssetAsync);
        group.MapPost("/{assetId:guid}/reaction", ReactToAssetAsync);

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
            UniqueName = request.UniqueName,
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
    private static Task<IResult> GetAssetDetailsAsync([AsParameters] GetAssetDetailsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetAssetCommentsAsync([AsParameters] GetAssetCommentsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> CreateAssetCommentAsync(CreateAssetCommentCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetCommentAsync([FromBody] DeleteAssetCommentCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> BookmarkAssetAsync(BookmarkAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> RemoveBookmarkAsync([FromBody] RemoveBookmarkCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> FollowAssetAsync(FollowAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> UnfollowAssetAsync([FromBody] UnfollowAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> ReactToAssetAsync(ReactToAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
}
