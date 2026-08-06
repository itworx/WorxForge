using System.Security.Claims;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Commands.Library;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Assets;
using WorxForge.Domain.Queries.Explore;
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
        group.MapGet("/search", SearchAssetsAsync)
            .WithName("SearchAssets")
            .WithSummary("Search knowledge assets")
            .WithDescription("Keyword search over Title/Description plus exact hashtag matching (#tag), combined with the same filters as Explore.")
            .Produces<PagedResultDto<AssetSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
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

    private static Task<IResult> CreateAssetAsync(CreateAssetCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
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
