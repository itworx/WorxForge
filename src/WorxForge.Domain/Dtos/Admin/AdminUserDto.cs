using WorxForge.Domain.Enums;

namespace WorxForge.Domain.Dtos.Admin;

public sealed class AdminUserDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}

