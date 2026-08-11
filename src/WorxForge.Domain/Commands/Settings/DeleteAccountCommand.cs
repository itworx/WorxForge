namespace WorxForge.Domain.Commands.Settings;

public sealed class DeleteAccountCommand
{
    public Guid UserId { get; init; }
    public bool Confirmed { get; init; }
}

