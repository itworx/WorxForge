namespace WorxForge.Domain.Commands.Profile;

public sealed class UpdateMyProfileCommand
{
    public Guid UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
}

