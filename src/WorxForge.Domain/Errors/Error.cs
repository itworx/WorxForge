namespace WorxForge.Domain.Errors;

/// <summary>A single (Code, Message, Type) triple; construct these only via <see cref="Errors"/> so a code never has more than one wording.</summary>
public sealed record Error(string Code, string Message, ErrorType Type);
