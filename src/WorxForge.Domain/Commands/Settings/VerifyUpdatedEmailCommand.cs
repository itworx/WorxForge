namespace WorxForge.Domain.Commands.Settings;

public sealed class VerifyUpdatedEmailCommand
{
    public Guid UserId { get; init; }
    public string Token { get; init; } = string.Empty;
}

