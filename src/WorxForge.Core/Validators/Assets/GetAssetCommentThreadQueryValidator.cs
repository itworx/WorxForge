using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetAssetCommentThreadQueryValidator : IRequestValidator<GetAssetCommentThreadQuery>
{
    public Task ValidateAsync(GetAssetCommentThreadQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.CommentId == Guid.Empty)
            throw new ValidationException("Comment identifier is required.");

        return Task.CompletedTask;
    }
}