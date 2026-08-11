using System.Security.Claims;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Admin;
using Microsoft.AspNetCore.Mvc;
using WorxForge.API.Models.Requests.Admin;

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
        group.MapGet("/reports", GetAdminReportsAsync)
            .RequireAuthorization(PolicyConstants.AdminOnly);
        group.MapPut("/reports/{reportId:guid}", ResolveReportByAdminAsync)
            .RequireAuthorization(PolicyConstants.AdminOnly);
        return group;
    }

    private static Task<IResult> GetAdminUsersAsync([AsParameters] GetAdminUsersQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> CreateUserByAdminAsync(CreateUserByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteUserByAdminAsync([FromBody] DeleteUserByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static Task<IResult> GetAdminAssetsAsync([AsParameters] GetAdminAssetsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteAssetByAdminAsync([FromBody] DeleteAssetByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> GetAdminCommentsAsync([AsParameters] GetAdminCommentsQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    private static Task<IResult> DeleteCommentByAdminAsync([FromBody] DeleteCommentByAdminCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static async Task<IResult> GetAdminReportsAsync(
        [AsParameters] GetAdminReportsQuery query,
        IRequestValidator<GetAdminReportsQuery> validator,
        IQueryHandler<GetAdminReportsQuery, PagedResultDto<AdminReportDto>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAsync(query, cancellationToken);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (RequestValidationException validationException)
        {
            return Results.ValidationProblem(validationException.Errors);
        }
    }

    private static async Task<IResult> ResolveReportByAdminAsync(
        Guid reportId,
        [FromBody] ResolveReportByAdminRequest request,
        ClaimsPrincipal user,
        IRequestValidator<ResolveReportByAdminCommand> validator,
        ICommandHandler<ResolveReportByAdminCommand> handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimConstants.UserId);
        if (!Guid.TryParse(userIdValue, out var adminUserId))
        {
            return Results.Unauthorized();
        }

        var command = new ResolveReportByAdminCommand
        {
            ReportId = reportId,
            AdminUserId = adminUserId,
            NewStatus = request.NewStatus
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
