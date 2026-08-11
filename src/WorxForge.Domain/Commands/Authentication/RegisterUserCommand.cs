namespace WorxForge.Domain.Commands.Authentication;

public sealed class RegisterUserCommand
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PasswordConfirm { get; init; } = string.Empty;
}

