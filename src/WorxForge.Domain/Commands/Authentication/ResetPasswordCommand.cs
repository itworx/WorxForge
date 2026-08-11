namespace WorxForge.Domain.Commands.Authentication;

public sealed class ResetPasswordCommand
{
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string NewPasswordConfirm { get; init; } = string.Empty;
}

