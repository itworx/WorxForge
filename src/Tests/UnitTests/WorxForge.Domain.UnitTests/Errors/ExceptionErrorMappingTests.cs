using FluentAssertions;
using WorxForge.Domain.Errors;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Domain.UnitTests.Errors;

public sealed class ExceptionErrorMappingTests
{
    [Fact]
    public void NotFoundException_DefaultsToResourceNotFoundCode_AndNotFoundType()
    {
        var exception = new NotFoundException("Widget was not found.");

        exception.Code.Should().Be(ErrorCodes.Resource.NotFound);
        exception.Type.Should().Be(ErrorType.NotFound);
        exception.Message.Should().Be("Widget was not found.");
    }

    [Fact]
    public void NotFoundException_AcceptsExplicitCode()
    {
        var exception = new NotFoundException("User was not found.", ErrorCodes.User.NotFound);

        exception.Code.Should().Be(ErrorCodes.User.NotFound);
        exception.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void ConflictException_DefaultsToResourceConflictCode_AndConflictType()
    {
        var exception = new ConflictException("Already exists.");

        exception.Code.Should().Be(ErrorCodes.Resource.Conflict);
        exception.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void ForbiddenException_DefaultsToAuthorizationForbiddenCode_AndForbiddenType()
    {
        var exception = new ForbiddenException("Not allowed.");

        exception.Code.Should().Be(ErrorCodes.Authorization.Forbidden);
        exception.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public void ValidationException_DefaultsToValidationFailedCode_AndValidationType()
    {
        var exception = new ValidationException("Field is required.");

        exception.Code.Should().Be(ErrorCodes.Validation.Failed);
        exception.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void UnauthorizedException_DefaultsToInvalidCredentialsCode_AndUnauthorizedType()
    {
        var exception = new UnauthorizedException("Token is invalid.");

        exception.Code.Should().Be(ErrorCodes.Authentication.InvalidCredentials);
        exception.Type.Should().Be(ErrorType.Unauthorized);
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
