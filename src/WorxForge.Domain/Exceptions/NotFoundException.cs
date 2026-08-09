using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class NotFoundException(Error error) : WorxForgeException(error);
