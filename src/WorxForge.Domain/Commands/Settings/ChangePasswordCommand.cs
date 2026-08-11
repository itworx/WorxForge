namespace WorxForge.Domain.Commands.Settings;

public sealed class ChangePasswordCommand
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string NewPasswordConfirm { get; init; } = string.Empty;
}

