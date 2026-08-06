namespace WorxForge.Domain.Constants;

// Bookmarks are a stronger intent signal than likes; domain match matters more than exact type match.
public static class AssetRecommendationConstants
{
    public const int LikeWeight = 1;
    public const int BookmarkWeight = 2;
    public const int DomainMatchWeight = 3;
    public const int TypeMatchWeight = 1;
}
