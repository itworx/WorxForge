using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Assets;

public sealed class CreateAssetCommandValidator : IRequestValidator<CreateAssetCommand>
{
    private const int TitleMaxLength = 200;
    private const int DescriptionMaxLength = 1000;
    private const int TagMaxLength = 50;
    private const int TagsMaxCount = 20;

    public Task ValidateAsync(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AuthorId == Guid.Empty)
            throw new ValidationException("Author is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        if (request.Title.Length > TitleMaxLength)
            throw new ValidationException($"Title must not exceed {TitleMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ValidationException("Description is required.");

        if (request.Description.Length > DescriptionMaxLength)
            throw new ValidationException($"Description must not exceed {DescriptionMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.MarkdownContent))
            throw new ValidationException("Markdown content is required.");

        if (request.AssetTypeId == Guid.Empty)
            throw new ValidationException("Asset type is required.");

        if (request.DomainId == Guid.Empty)
            throw new ValidationException("Domain is required.");

        if (request.UseCaseId == Guid.Empty)
            throw new ValidationException("Use case is required.");

        if (request.ToolIds.Count == 0)
            throw new ValidationException("At least one compatible AI tool is required.");

        if (request.ToolIds.Count != request.ToolIds.Distinct().Count())
            throw new ValidationException("Duplicate tool IDs are not allowed.");

        if (request.Tags.Count > TagsMaxCount)
            throw new ValidationException($"A maximum of {TagsMaxCount} tags is allowed.");

        foreach (var tag in request.Tags)
        {
            if (tag.Trim().Length > TagMaxLength)
                throw new ValidationException($"Each tag must not exceed {TagMaxLength} characters.");
        }

        return Task.CompletedTask;
    }
}
