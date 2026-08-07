using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Comments;

public sealed class UpdateAssetCommentCommandValidator : IRequestValidator<UpdateAssetCommentCommand>
{
    private const int ContentMaxLength = 2000;

    public Task ValidateAsync(UpdateAssetCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.CommentId == Guid.Empty)
            throw new ValidationException("Comment identifier is required.");

        if (request.RequestingUserId == Guid.Empty)
            throw new ValidationException("Requesting user is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Content is required.");

        if (request.Content.Length > ContentMaxLength)
            throw new ValidationException($"Content must not exceed {ContentMaxLength} characters.");

        return Task.CompletedTask;
    }
}