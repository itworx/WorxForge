using FluentAssertions;
using WorxForge.Domain.Exceptions;
using ErrorsCatalog = WorxForge.Domain.Errors.Errors;
using ErrorCodes = WorxForge.Domain.Errors.ErrorCodes;
using ErrorType = WorxForge.Domain.Errors.ErrorType;

namespace WorxForge.Domain.UnitTests.Errors;

public sealed class ExceptionErrorMappingTests
{
    [Fact]
    public void NotFoundException_CarriesCodeAndMessage_FromCatalogError()
    {
        var exception = new NotFoundException(ErrorsCatalog.UserNotFound);

        exception.Code.Should().Be(ErrorCodes.User.NotFound);
        exception.Type.Should().Be(ErrorType.NotFound);
        exception.Message.Should().Be(ErrorsCatalog.UserNotFound.Message);
    }

    [Fact]
    public void ConflictException_CarriesCodeAndMessage_FromCatalogError()
    {
        var exception = new ConflictException(ErrorsCatalog.UserUsernameAlreadyInUse);

        exception.Code.Should().Be(ErrorCodes.User.UsernameAlreadyInUse);
        exception.Type.Should().Be(ErrorType.Conflict);
        exception.Message.Should().Be(ErrorsCatalog.UserUsernameAlreadyInUse.Message);
    }

    [Fact]
    public void ForbiddenException_CarriesCodeAndMessage_FromCatalogError()
    {
        var exception = new ForbiddenException(ErrorsCatalog.AuthorizationForbidden);

        exception.Code.Should().Be(ErrorCodes.Authorization.Forbidden);
        exception.Type.Should().Be(ErrorType.Forbidden);
        exception.Message.Should().Be(ErrorsCatalog.AuthorizationForbidden.Message);
    }

    [Fact]
    public void ValidationException_DefaultsToValidationFailedCode_AndValidationType()
    {
        var exception = new ValidationException("Field is required.");

        exception.Code.Should().Be(ErrorCodes.Validation.Failed);
        exception.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void UnauthorizedException_CarriesCodeAndMessage_FromCatalogError()
    {
        var exception = new UnauthorizedException(ErrorsCatalog.AuthenticationInvalidCredentials);

        exception.Code.Should().Be(ErrorCodes.Authentication.InvalidCredentials);
        exception.Type.Should().Be(ErrorType.Unauthorized);
        exception.Message.Should().Be(ErrorsCatalog.AuthenticationInvalidCredentials.Message);
    }

    [Fact]
    public void RequestValidationException_CarriesFieldErrors_WithValidationFailedCode()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["email"] = ["Email is required."]
        };

        var exception = new RequestValidationException(errors);

        exception.Code.Should().Be(ErrorCodes.Validation.Failed);
        exception.Type.Should().Be(ErrorType.Validation);
        exception.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void WorxForgeException_WithoutExplicitCode_DefaultsToGeneralUnexpected()
    {
        var exception = new WorxForgeException("Something went wrong.");

        exception.Code.Should().Be(ErrorCodes.General.Unexpected);
        exception.Type.Should().Be(ErrorType.Unexpected);
    }
}
