namespace WorxForge.Domain.Exceptions;

public sealed class ForbiddenException(string message) : WorxForgeException(message);
