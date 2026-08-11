namespace WorxForge.Domain.Commands.Assets;

public sealed class UpdateAssetCommand
{
    public Guid AssetId { get; init; }
    public Guid RequestingUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid AssetTypeId { get; init; }
    public Guid DomainId { get; init; }
    public Guid UseCaseId { get; init; }
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Version { get; init; }
    public string MarkdownContent { get; init; } = string.Empty;
}

