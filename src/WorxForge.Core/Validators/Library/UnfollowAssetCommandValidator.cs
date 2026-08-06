using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Library;

namespace WorxForge.Core.Validators.Library;

public sealed class UnfollowAssetCommandValidator : IRequestValidator<UnfollowAssetCommand>
{
    public Task ValidateAsync(UnfollowAssetCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(request.UserId));
        }

        if (request.AssetId == Guid.Empty)
        {
            throw new ArgumentException("AssetId is required.", nameof(request.AssetId));
        }

        return Task.CompletedTask;
    }
}
