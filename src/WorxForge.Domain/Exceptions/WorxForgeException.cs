using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public class WorxForgeException : Exception
{
    public WorxForgeException()
    {
        Code = ErrorCodes.General.Unexpected;
        Type = ErrorType.Unexpected;
    }

    public WorxForgeException(string? message) : base(message)
    {
        Code = ErrorCodes.General.Unexpected;
        Type = ErrorType.Unexpected;
    }

    public WorxForgeException(string? message, Exception? innerException) : base(message, innerException)
    {
        Code = ErrorCodes.General.Unexpected;
        Type = ErrorType.Unexpected;
    }

    protected WorxForgeException(Error error) : base(error.Message)
    {
        Code = error.Code;
        Type = error.Type;
    }

    protected WorxForgeException(Error error, Exception? innerException) : base(error.Message, innerException)
    {
        Code = error.Code;
        Type = error.Type;
    }

    /// <summary>Stable, machine-readable error code (e.g. "User.NotFound"), never an HTTP status code.</summary>
    public string Code { get; }

    public ErrorType Type { get; }
}
