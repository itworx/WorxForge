using ErrorsCatalog = WorxForge.Domain.Errors.Errors;

namespace WorxForge.Domain.Exceptions;

public sealed class RequestValidationException : WorxForgeException
{
    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(ErrorsCatalog.ValidationFailed)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}