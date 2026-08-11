namespace WorxForge.API.Models.Responses.Common;

/// <summary>Standard error contract returned by the global exception handler for every non-2xx response.</summary>
public sealed record ApiErrorResponse(string Code, string Message, object? Details = null);
