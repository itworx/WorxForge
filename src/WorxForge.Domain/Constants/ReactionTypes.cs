namespace WorxForge.Domain.Constants;

public static class ReactionTypes
{
    public const string Like = "Like";
    public const string Dislike = "Dislike";

    public static bool IsSupported(string? reactionType) =>
        string.Equals(reactionType, Like, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reactionType, Dislike, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string reactionType)
    {
        if (string.Equals(reactionType, Like, StringComparison.OrdinalIgnoreCase))
        {
            return Like;
        }

        if (string.Equals(reactionType, Dislike, StringComparison.OrdinalIgnoreCase))
        {
            return Dislike;
        }

        throw new ArgumentOutOfRangeException(nameof(reactionType), reactionType, "Unsupported reaction type.");
    }
}