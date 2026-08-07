using WorxForge.Domain.Commands.Admin;
using WorxForge.Domain.Queries.Admin;
using Microsoft.AspNetCore.Mvc;

namespace WorxForge.API.EndpointGroups;

public static class AdminEndpointGroup
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin");
        group.MapGet("/users", GetAdminUsersAsync);
        group.MapPost("/users", CreateUserByAdminAsync);
        group.MapDelete("/users/{userId:guid}", DeleteUserByAdminAsync);
        group.MapGet("/assets", GetAdminAssetsAsync);
        group.MapDelete("/assets/{assetId:guid}", DeleteAssetByAdminAsync);
        group.MapGet("/comments", GetAdminCommentsAsync);
        group.MapDelete("/comments/{commentId:guid}", DeleteCommentByAdminAsync);
        group.MapGet("/reports", GetAdminReportsAsync);
        group.MapPut("/reports/{reportId:guid}", ResolveReportByAdminAsync);
        return group;
    }

    private static Task<IResult> GetAdminUsersAsync([AsParameters] GetAdminUsersQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> CreateUserByAdminAsync(CreateUserByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteUserByAdminAsync([FromBody] DeleteUserByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static Task<IResult> GetAdminAssetsAsync([AsParameters] GetAdminAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetByAdminAsync([FromBody] DeleteAssetByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetAdminCommentsAsync([AsParameters] GetAdminCommentsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteCommentByAdminAsync([FromBody] DeleteCommentByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetAdminReportsAsync([AsParameters] GetAdminReportsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> ResolveReportByAdminAsync(ResolveReportByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
}
