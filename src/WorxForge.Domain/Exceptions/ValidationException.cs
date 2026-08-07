namespace WorxForge.Domain.Exceptions;

public sealed class ValidationException(string message) : WorxForgeException(message);
