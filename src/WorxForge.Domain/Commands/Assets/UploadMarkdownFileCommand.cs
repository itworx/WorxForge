namespace WorxForge.Domain.Commands.Assets;

public sealed class UploadMarkdownFileCommand
{
    public byte[] FileContent { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
}
