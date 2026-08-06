using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Comments;

public sealed class RemoveAssetReactionCommandValidator : IRequestValidator<RemoveAssetReactionCommand>
{
    public Task ValidateAsync(RemoveAssetReactionCommand request, CancellationToken cancellationToken)
    {
        Dictionary<string, string[]> errors = [];

        if (request.UserId == Guid.Empty)
        {
            errors[nameof(request.UserId)] = ["UserId is required."];
        }

        if (request.AssetId == Guid.Empty)
        {
            errors[nameof(request.AssetId)] = ["AssetId is required."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return Task.CompletedTask;
    }
}
