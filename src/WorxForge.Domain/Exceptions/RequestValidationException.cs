using WorxForge.Domain.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class RequestValidationException : WorxForgeException
{
    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(ErrorCodes.Validation.Failed, "One or more validation errors occurred.", ErrorType.Validation)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}