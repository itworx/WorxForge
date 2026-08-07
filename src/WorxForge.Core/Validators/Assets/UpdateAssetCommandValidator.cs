using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Assets;

public sealed class UpdateAssetCommandValidator : IRequestValidator<UpdateAssetCommand>
{
    public Task ValidateAsync(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.RequestingUserId == Guid.Empty)
            throw new ValidationException("Requesting user is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        if (request.Title.Length > AssetValidationConstants.TitleMaxLength)
            throw new ValidationException($"Title must not exceed {AssetValidationConstants.TitleMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ValidationException("Description is required.");

        if (request.Description.Length > AssetValidationConstants.DescriptionMaxLength)
            throw new ValidationException($"Description must not exceed {AssetValidationConstants.DescriptionMaxLength} characters.");

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

        if (request.Tags.Count > AssetValidationConstants.TagsMaxCount)
            throw new ValidationException($"A maximum of {AssetValidationConstants.TagsMaxCount} tags is allowed.");

        foreach (var tag in request.Tags)
        {
            if (tag.Trim().Length > AssetValidationConstants.TagMaxLength)
                throw new ValidationException($"Each tag must not exceed {AssetValidationConstants.TagMaxLength} characters.");
        }

        return Task.CompletedTask;
    }
}
