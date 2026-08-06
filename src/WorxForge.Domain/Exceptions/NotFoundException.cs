namespace WorxForge.Domain.Exceptions;

public sealed class NotFoundException(string message) : WorxForgeException(message);
