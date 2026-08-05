using WorxForge.Domain.Commands.Profile;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.API.EndpointGroups;

public static class ProfileEndpointGroup
{
    public static RouteGroupBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile");
        group.MapGet("/me", GetMyProfileAsync);
        group.MapGet("/{userId:guid}", GetPublicProfileAsync);
        group.MapGet("/me/statistics", GetMyProfileStatisticsAsync);
        group.MapPut("/me", UpdateMyProfileAsync);
        return group;
    }

    private static Task<IResult> GetMyProfileAsync([AsParameters] GetMyProfileQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetPublicProfileAsync([AsParameters] GetPublicProfileQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetMyProfileStatisticsAsync([AsParameters] GetMyProfileStatisticsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> UpdateMyProfileAsync(UpdateMyProfileCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
}
