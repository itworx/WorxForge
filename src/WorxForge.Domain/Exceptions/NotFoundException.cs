using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class NotFoundException(string message, string code = ErrorCodes.Resource.NotFound)
    : WorxForgeException(code, message, ErrorType.NotFound);
