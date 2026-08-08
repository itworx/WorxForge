using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class ForbiddenException(Error error) : WorxForgeException(error);
