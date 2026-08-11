namespace WorxForge.Domain.Constants;

public static class ReportReasonConstants
{
    public const string IncorrectClassification = "Incorrect classification";
    public const string Duplicate = "Duplicate";
    public const string Obsolete = "Obsolete";
    public const string UnsafeContent = "Unsafe content";
    public const string Other = "Other";

    public static readonly IReadOnlySet<string> AllowedReasons = new HashSet<string>(StringComparer.Ordinal)
    {
        IncorrectClassification,
        Duplicate,
        Obsolete,
        UnsafeContent,
        Other
    };
}
