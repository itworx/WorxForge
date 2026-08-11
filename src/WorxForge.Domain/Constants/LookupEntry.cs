namespace WorxForge.Domain.Constants;

// A fixed lookup row: Id and Name are always defined together so they can never drift apart.
public readonly record struct LookupEntry(Guid Id, string Name);
