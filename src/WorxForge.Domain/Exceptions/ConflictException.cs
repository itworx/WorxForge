using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class ConflictException(string message, string code = ErrorCodes.Resource.Conflict)
    : WorxForgeException(code, message, ErrorType.Conflict);
