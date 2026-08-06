namespace WorxForge.Domain.Exceptions;

public sealed class ConflictException(string message) : WorxForgeException(message);
