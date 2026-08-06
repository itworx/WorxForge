using System.Security.Claims;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Assets;
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
        IQueryHandler<GetSavedAssetsQuery, IReadOnlyList<AssetSummaryDto>> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var savedAssets = await handler.HandleAsync(new GetSavedAssetsQuery { UserId = userId }, cancellationToken);
        return Results.Ok(savedAssets);
    }
    private static Task<IResult> GetRecentlyUsedAssetsAsync([AsParameters] GetRecentlyUsedAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetFollowingAssetsAsync([AsParameters] GetFollowingAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetMyAssetsAsync([AsParameters] GetMyAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetMyAssetDetailsAsync([AsParameters] GetMyAssetDetailsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
}
