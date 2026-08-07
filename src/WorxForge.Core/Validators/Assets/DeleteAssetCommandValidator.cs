using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Assets;

public sealed class DeleteAssetCommandValidator : IRequestValidator<DeleteAssetCommand>
{
    public Task ValidateAsync(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.RequestingUserId == Guid.Empty)
            throw new ValidationException("Requesting user is required.");

        return Task.CompletedTask;
    }
}
