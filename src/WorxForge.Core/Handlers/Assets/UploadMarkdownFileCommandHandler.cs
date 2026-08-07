using System.Text;
using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Handlers.Assets;

public sealed class UploadMarkdownFileCommandHandler(
    IRequestValidator<UploadMarkdownFileCommand> validator) : ICommandHandler<UploadMarkdownFileCommand, UploadMarkdownResponse>
{
    public async Task<UploadMarkdownResponse> HandleAsync(UploadMarkdownFileCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);

        string markdownContent;
        try
        {
            // Throw on invalid bytes instead of silently replacing them with the replacement character.
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            markdownContent = utf8.GetString(command.FileContent).Trim();
        }
        catch (DecoderFallbackException)
        {
            throw new ValidationException("The uploaded file is not valid UTF-8.");
        }

        if (string.IsNullOrEmpty(markdownContent))
            throw new ValidationException("The uploaded file contains no markdown content.");

        return new UploadMarkdownResponse { MarkdownContent = markdownContent };
    }
}
