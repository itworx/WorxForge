using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Comments;

public sealed class DeleteAssetCommentCommandValidator : IRequestValidator<DeleteAssetCommentCommand>
{
    public Task ValidateAsync(DeleteAssetCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.CommentId == Guid.Empty)
            throw new ValidationException("Comment identifier is required.");

        if (request.RequestingUserId == Guid.Empty)
            throw new ValidationException("Requesting user is required.");

        return Task.CompletedTask;
    }
}
