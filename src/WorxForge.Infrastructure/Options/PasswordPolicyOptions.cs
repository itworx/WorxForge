namespace WorxForge.Infrastructure.Options;

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int MinLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireNumber { get; set; }
    public bool RequireSpecialCharacter { get; set; }
}
