namespace WorxForge.Domain.Dtos.Authentication;

public sealed class AuthTokenDto
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

