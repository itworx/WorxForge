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

    protected WorxForgeException(string code, string? message, ErrorType type) : base(message)
    {
        Code = code;
        Type = type;
    }

    protected WorxForgeException(string code, string? message, ErrorType type, Exception? innerException) : base(message, innerException)
    {
        Code = code;
        Type = type;
    }

    /// <summary>Stable, machine-readable error code (e.g. "User.NotFound"), never an HTTP status code.</summary>
    public string Code { get; }

    public ErrorType Type { get; }
}
