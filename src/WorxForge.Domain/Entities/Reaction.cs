namespace WorxForge.Domain.Entities;

public sealed class Reaction
{
    public Guid AssetId { get; set; }
    public Guid UserId { get; set; }
    public string ReactionType { get; set; } = string.Empty;
}
