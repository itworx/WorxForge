namespace WorxForge.Domain.Errors;

/// <summary>Error category used by the API layer to map an error to an HTTP status code; carries no HTTP concepts itself.</summary>
public enum ErrorType
{
    Unexpected,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}
