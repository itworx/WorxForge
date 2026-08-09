using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class UnauthorizedException : WorxForgeException
{
    public UnauthorizedException(Error error) : base(error)
    {
    }

    public UnauthorizedException(Error error, Exception innerException) : base(error, innerException)
    {
    }
}
