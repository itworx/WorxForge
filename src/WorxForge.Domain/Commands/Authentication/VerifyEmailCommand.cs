namespace WorxForge.Domain.Commands.Authentication;

public sealed class VerifyEmailCommand
{
    public string Token { get; init; } = string.Empty;
}

