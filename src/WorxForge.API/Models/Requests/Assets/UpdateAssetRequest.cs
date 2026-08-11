namespace WorxForge.API.Models.Requests.Assets;

public sealed class UpdateAssetRequest
{
	public string Title { get; init; } = string.Empty;
	public string? Description { get; init; }
	public string MarkdownContent { get; init; } = string.Empty;
	public Guid AssetTypeId { get; init; }
	public Guid DomainId { get; init; }
	public Guid UseCaseId { get; init; }
	public IReadOnlyList<Guid> ToolIds { get; init; } = [];
	public IReadOnlyList<string> Tags { get; init; } = [];
}

