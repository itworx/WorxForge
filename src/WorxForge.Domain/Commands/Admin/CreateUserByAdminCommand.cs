using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Commands.Admin;

public sealed class CreateUserByAdminCommand
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
}

