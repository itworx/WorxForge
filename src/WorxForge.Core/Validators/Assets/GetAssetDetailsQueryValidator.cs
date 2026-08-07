using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetAssetDetailsQueryValidator : IRequestValidator<GetAssetDetailsQuery>
{
    public Task ValidateAsync(GetAssetDetailsQuery request, CancellationToken cancellationToken)
    {
        if (request.AssetId == Guid.Empty)
        {
            throw new ArgumentException("AssetId is required.", nameof(request.AssetId));
        }

        if (request.RequestingUserId == Guid.Empty)
        {
            throw new ArgumentException("RequestingUserId is required.", nameof(request.RequestingUserId));
        }

        return Task.CompletedTask;
    }
}
