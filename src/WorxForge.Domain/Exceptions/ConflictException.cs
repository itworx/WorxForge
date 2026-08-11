using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class ConflictException(Error error) : WorxForgeException(error);
