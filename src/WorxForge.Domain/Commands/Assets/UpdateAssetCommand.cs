namespace WorxForge.Domain.Commands.Assets;

public sealed class UpdateAssetCommand
{
    public Guid AssetId { get; init; }
    public Guid RequestingUserId { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid AssetTypeId { get; init; }
    public Guid DomainId { get; init; }
    public Guid UseCaseId { get; init; }
    public IReadOnlyList<Guid> ToolIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string MarkdownContent { get; init; } = string.Empty;
}

