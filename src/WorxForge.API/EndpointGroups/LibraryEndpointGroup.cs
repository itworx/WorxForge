using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Assets;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.API.EndpointGroups;

public static class LibraryEndpointGroup
{
    public static RouteGroupBuilder MapLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/library");
        group.MapGet("/saved", GetSavedAssetsAsync);
        group.MapGet("/recently-used", GetRecentlyUsedAssetsAsync);
        group.MapGet("/following", GetFollowingAssetsAsync);
        group.MapGet("/my-assets", GetMyAssetsAsync);
        group.MapGet("/my-assets/{assetId:guid}", GetMyAssetDetailsAsync);
        return group;
    }

    private static async Task<IResult> GetSavedAssetsAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetSavedAssetsQuery, PagedResultDto<AssetSummaryDto>> handler,
        [FromQuery(Name = "Page")] int? page,
        [FromQuery(Name = "PageSize")] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var defaultQuery = new GetSavedAssetsQuery { UserId = userId };
        var query = new GetSavedAssetsQuery
        {
            UserId = userId,
            Page = page ?? defaultQuery.Page,
            PageSize = pageSize ?? defaultQuery.PageSize
        };

        var savedAssets = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(savedAssets);
    }
    private static Task<IResult> GetRecentlyUsedAssetsAsync([AsParameters] GetRecentlyUsedAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetFollowingAssetsAsync([AsParameters] GetFollowingAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static async Task<IResult> GetMyAssetsAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetMyAssetsQuery, PagedResultDto<MyAssetDto>> handler,
        [FromQuery(Name = "Page")] int? page,
        [FromQuery(Name = "PageSize")] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var defaultQuery = new GetMyAssetsQuery { UserId = userId };
        var query = new GetMyAssetsQuery
        {
            UserId = userId,
            Page = page ?? defaultQuery.Page,
            PageSize = pageSize ?? defaultQuery.PageSize
        };

        var myAssets = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(myAssets);
    }
    private static Task<IResult> GetMyAssetDetailsAsync([AsParameters] GetMyAssetDetailsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
}
