namespace WorxForge.API.Models.Responses.Common;

/// <summary>Error contract for field-level validation failures; Details keys are field names.</summary>
public sealed record ValidationErrorResponse(string Code, string Message, IReadOnlyDictionary<string, string[]> Details);
