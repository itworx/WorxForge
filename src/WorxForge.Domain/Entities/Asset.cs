namespace WorxForge.Domain.Entities;

public sealed class Asset
{
    public Guid AssetId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MarkdownContent { get; set; } = string.Empty;
    public Guid AssetTypeId { get; set; }
    public Guid DomainId { get; set; }
    public Guid UseCaseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
