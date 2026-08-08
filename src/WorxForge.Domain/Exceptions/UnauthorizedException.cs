using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class UnauthorizedException : WorxForgeException
{
    public UnauthorizedException(string message, string code = ErrorCodes.Authentication.InvalidCredentials)
        : base(code, message, ErrorType.Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException, string code = ErrorCodes.Authentication.InvalidCredentials)
        : base(code, message, ErrorType.Unauthorized, innerException)
    {
    }
}
