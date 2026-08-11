namespace WorxForge.Infrastructure.Options;

public sealed class JwtAccessTokenOptions
{
    public const string SectionName = "JwtAccessToken";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}
