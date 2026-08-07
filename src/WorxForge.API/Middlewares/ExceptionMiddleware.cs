using System.Net;
using System.Text.Json;
using WorxForge.Domain.Exceptions;

namespace WorxForge.API.Middlewares;

public sealed class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (WorxForgeException ex)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            throw;
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
