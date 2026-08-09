using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using WorxForge.API.Middlewares;
using WorxForge.API.Models.Responses.Common;
using WorxForge.Domain.Errors;
using WorxForge.Domain.Exceptions;

namespace WorxForge.API.UnitTests.Middlewares;

public sealed class GlobalExceptionHandlerTests
{
    private static readonly GlobalExceptionHandler Handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    public static IEnumerable<object[]> KnownExceptionCases()
    {
        yield return [new ValidationException("Name is required."), StatusCodes.Status400BadRequest, ErrorCodes.Validation.Failed];
        yield return [new UnauthorizedException(Errors.AuthenticationInvalidCredentials), StatusCodes.Status401Unauthorized, ErrorCodes.Authentication.InvalidCredentials];
        yield return [new ForbiddenException(Errors.AuthorizationForbidden), StatusCodes.Status403Forbidden, ErrorCodes.Authorization.Forbidden];
        yield return [new NotFoundException(Errors.UserNotFound), StatusCodes.Status404NotFound, ErrorCodes.User.NotFound];
        yield return [new ConflictException(Errors.UserUsernameAlreadyInUse), StatusCodes.Status409Conflict, ErrorCodes.User.UsernameAlreadyInUse];
    }

    [Theory]
    [MemberData(nameof(KnownExceptionCases))]
    public async Task TryHandleAsync_MapsKnownExceptions_ToExpectedStatusAndCode(Exception exception, int expectedStatusCode, string expectedCode)
    {
        var (statusCode, body) = await InvokeAsync(exception);

        statusCode.Should().Be(expectedStatusCode);
        body.GetProperty("code").GetString().Should().Be(expectedCode);
        body.GetProperty("message").GetString().Should().Be(exception.Message);
    }

    [Fact]
    public async Task TryHandleAsync_RequestValidationException_ReturnsStructuredFieldErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["email"] = ["Email is required."],
            ["name"] = ["Name is required."]
        };
        var exception = new RequestValidationException(errors);

        var (statusCode, body) = await InvokeAsync(exception);

        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.GetProperty("code").GetString().Should().Be(ErrorCodes.Validation.Failed);
        var details = body.GetProperty("details");
        details.GetProperty("email")[0].GetString().Should().Be("Email is required.");
        details.GetProperty("name")[0].GetString().Should().Be("Name is required.");
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500WithGenericMessage_AndNoInternalDetails()
    {
        var exception = new InvalidOperationException("Connection string 'Server=secret-db;Password=hunter2' failed.");

        var (statusCode, body) = await InvokeAsync(exception);

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.GetProperty("code").GetString().Should().Be(ErrorCodes.General.Unexpected);
        var message = body.GetProperty("message").GetString();
        message.Should().NotBeNullOrEmpty();
        message.Should().NotContain("secret-db");
        message.Should().NotContain("hunter2");
        message.Should().NotContain(exception.GetType().FullName);
        body.TryGetProperty("stackTrace", out _).Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedTypeWorxForgeException_Returns500WithGenericMessage_AndNoInternalDetails()
    {
        // A plain WorxForgeException (e.g. thrown for missing startup configuration) still carries Type.Unexpected;
        // it must never leak its own message to the client even though it is a "known" exception type.
        var exception = new WorxForgeException("JwtAccessToken configuration is incomplete: missing SigningKey.");

        var (statusCode, body) = await InvokeAsync(exception);

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.GetProperty("code").GetString().Should().Be(ErrorCodes.General.Unexpected);
        body.GetProperty("message").GetString().Should().Be(Errors.GeneralUnexpected.Message);
        body.GetProperty("message").GetString().Should().NotContain("SigningKey");
    }

    private static async Task<(int StatusCode, JsonElement Body)> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();

        var handled = await Handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var document = JsonDocument.Parse(json);
        return (context.Response.StatusCode, document.RootElement.Clone());
    }
}
