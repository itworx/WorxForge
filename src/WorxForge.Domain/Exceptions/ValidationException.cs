using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class ValidationException(string message, string code = ErrorCodes.Validation.Failed)
    : WorxForgeException(new Error(code, message, ErrorType.Validation));
