using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Assets;

public sealed class UploadMarkdownFileCommandValidator : IRequestValidator<UploadMarkdownFileCommand>
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024;

    public Task ValidateAsync(UploadMarkdownFileCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.FileContent.Length == 0)
            throw new ValidationException("The uploaded file is empty.");

        if (request.FileContent.Length > MaxFileSizeBytes)
            throw new ValidationException("The uploaded file exceeds the 5 MB limit.");

        if (!Path.GetExtension(request.FileName).Equals(".md", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Only .md files are allowed.");

        return Task.CompletedTask;
    }
}
