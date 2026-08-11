namespace WorxForge.Domain.Commands.Settings;

public sealed class UpdateEmailAddressCommand
{
    public Guid UserId { get; init; }
    public string NewEmail { get; init; } = string.Empty;
}

