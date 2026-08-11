using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetAssetDetailsQueryValidator : IRequestValidator<GetAssetDetailsQuery>
{
    public Task ValidateAsync(GetAssetDetailsQuery request, CancellationToken cancellationToken)
    {
        if (request.AssetId == Guid.Empty)
        {
            throw new ValidationException("AssetId is required.");
        }

        if (request.RequestingUserId == Guid.Empty)
        {
            throw new ValidationException("RequestingUserId is required.");
        }

        return Task.CompletedTask;
    }
}
