using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Comments;

public sealed class CreateAssetCommentCommandValidator : IRequestValidator<CreateAssetCommentCommand>
{
    private const int ContentMaxLength = 2000;

    public Task ValidateAsync(CreateAssetCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.UserId == Guid.Empty)
            throw new ValidationException("User identifier is required.");

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.ParentCommentId.HasValue && request.ParentCommentId.Value == Guid.Empty)
            throw new ValidationException("Parent comment identifier is invalid.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Content is required.");

        if (request.Content.Length > ContentMaxLength)
            throw new ValidationException($"Content must not exceed {ContentMaxLength} characters.");

        return Task.CompletedTask;
    }
}
