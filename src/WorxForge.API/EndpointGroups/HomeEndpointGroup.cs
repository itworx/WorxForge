using System.Security.Claims;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Dtos.Home;
using WorxForge.Domain.Queries.Home;

namespace WorxForge.API.EndpointGroups;

public static class HomeEndpointGroup
{
    public static RouteGroupBuilder MapHomeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/home").WithTags("Home");
        group.MapGet("/dashboard", GetHomeDashboardAsync)
            .WithName("GetHomeDashboard")
            .WithSummary("Get the personalized Home dashboard")
            .WithDescription("Returns the authenticated user's Home dashboard, including useful this week, recently updated, recommended for you, popular across teams, and domain summaries.")
            .Produces<HomeDashboardDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        return group;
    }

    private static async Task<IResult> GetHomeDashboardAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetHomeDashboardQuery, HomeDashboardDto> handler,
        CancellationToken cancellationToken)
    {
        // UserId comes from the authenticated token, never from client input.
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Results.Unauthorized();
        }

        var dashboard = await handler.HandleAsync(new GetHomeDashboardQuery { UserId = userId }, cancellationToken);
        return Results.Ok(dashboard);
    }
}
