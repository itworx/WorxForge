using Microsoft.AspNetCore.Diagnostics;
using WorxForge.API.Models.Responses.Common;
using WorxForge.Domain.Errors;
using WorxForge.Domain.Exceptions;

namespace WorxForge.API.Middlewares;

/// <summary>
/// Single global entry point for turning exceptions into the API's error response contract.
/// Expected application failures (<see cref="WorxForgeException"/> and subtypes) are mapped to their
/// corresponding HTTP status via <see cref="ErrorType"/>; anything else is logged and reduced to a
/// generic 500 response so no internal detail (stack traces, DB errors, etc.) ever reaches the client.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, response) = BuildResponse(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Request {Method} {Path} failed with {Code}", httpContext.Request.Method, httpContext.Request.Path, response.Code);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private static (int StatusCode, ApiErrorResponse Response) BuildResponse(Exception exception) => exception switch
    {
        RequestValidationException validationException => (
            StatusCodes.Status400BadRequest,
            new ApiErrorResponse(validationException.Code, validationException.Message, validationException.Errors)),
        WorxForgeException { Type: ErrorType.Unexpected } => (
            StatusCodes.Status500InternalServerError,
            UnexpectedResponse),
        WorxForgeException worxForgeException => (
            MapStatusCode(worxForgeException.Type),
            new ApiErrorResponse(worxForgeException.Code, worxForgeException.Message)),
        _ => (
            StatusCodes.Status500InternalServerError,
            UnexpectedResponse)
    };

    private static readonly ApiErrorResponse UnexpectedResponse = new(Errors.GeneralUnexpected.Code, Errors.GeneralUnexpected.Message);

    private static int MapStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
