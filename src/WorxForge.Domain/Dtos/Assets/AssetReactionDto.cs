namespace WorxForge.Domain.Dtos.Assets;

public sealed class AssetReactionDto
{
    public Guid AssetId { get; init; }
    public int LikeCount { get; init; }
    public int DislikeCount { get; init; }
    public int TotalCount { get; init; }
    public string? CurrentUserReaction { get; init; }
}