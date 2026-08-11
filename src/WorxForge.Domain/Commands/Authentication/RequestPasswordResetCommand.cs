namespace WorxForge.Domain.Commands.Authentication;

public sealed class RequestPasswordResetCommand
{
    public string Email { get; init; } = string.Empty;
}

