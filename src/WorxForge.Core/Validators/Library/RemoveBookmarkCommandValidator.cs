using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Library;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Library;

public sealed class RemoveBookmarkCommandValidator : IRequestValidator<RemoveBookmarkCommand>
{
    public Task ValidateAsync(RemoveBookmarkCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.UserId == Guid.Empty)
            throw new ValidationException("User identifier is required.");

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        return Task.CompletedTask;
    }
}
