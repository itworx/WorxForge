using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetMyAssetDetailsQueryValidator : IRequestValidator<GetMyAssetDetailsQuery>
{
    public Task ValidateAsync(GetMyAssetDetailsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
