namespace WorxForge.Domain.Exceptions;

public class WorxForgeException : Exception
{
    public WorxForgeException()
    {
    }

    public WorxForgeException(string? message) : base(message)
    {
    }

    public WorxForgeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
