using System.Security.Claims;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.API.EndpointGroups;

public static class ExploreEndpointGroup
{
    public static RouteGroupBuilder MapExploreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/explore").WithTags("Explore");
        group.MapGet("/browse", BrowseAssetsAsync)
            .WithName("BrowseAssets")
            .WithSummary("Browse knowledge assets")
            .WithDescription("Browses published knowledge assets with optional type, domain, use case, tool, and freshness filters, sortable by relevance, helpful, added, updated, or discussed.")
            .Produces<PagedResultDto<AssetSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        group.MapGet("/filters", GetExploreFiltersAsync)
            .WithName("GetExploreFilters")
            .WithSummary("Get available Explore filters")
            .WithDescription("Returns the lookup values available for filtering Explore results: domains, asset types, use cases, tools, and tags.")
            .Produces<ExploreFiltersDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        return group;
    }

    private static async Task<IResult> BrowseAssetsAsync(
        [AsParameters] AssetFilters filters,
        ClaimsPrincipal user,
        IQueryHandler<BrowseAssetsQuery, PagedResultDto<AssetSummaryDto>> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new BrowseAssetsQuery
        {
            RequestingUserId = userId,
            AssetTypeId = filters.AssetTypeId,
            DomainId = filters.DomainId,
            UseCaseId = filters.UseCaseId,
            ToolId = filters.ToolId,
            Freshness = filters.Freshness,
            SortBy = filters.SortBy,
            Page = filters.Page ?? 1,
            PageSize = filters.PageSize ?? 20
        };

        var results = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(results);
    }

    private static async Task<IResult> GetExploreFiltersAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetExploreFiltersQuery, ExploreFiltersDto> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var filters = await handler.HandleAsync(new GetExploreFiltersQuery { RequestingUserId = userId }, cancellationToken);
        return Results.Ok(filters);
    }
}

