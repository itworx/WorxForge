namespace WorxForge.Domain.Commands.Authentication;

public sealed class LogoutUserCommand
{
    public Guid UserId { get; init; }
    public Guid SessionId { get; init; }
}

