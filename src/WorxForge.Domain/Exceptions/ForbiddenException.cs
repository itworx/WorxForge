using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class ForbiddenException(string message, string code = ErrorCodes.Authorization.Forbidden)
    : WorxForgeException(code, message, ErrorType.Forbidden);
