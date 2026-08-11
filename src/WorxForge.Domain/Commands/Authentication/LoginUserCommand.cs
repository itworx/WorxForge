namespace WorxForge.Domain.Commands.Authentication;

public sealed class LoginUserCommand
{
    public string IdToken { get; init; } = string.Empty;
}

