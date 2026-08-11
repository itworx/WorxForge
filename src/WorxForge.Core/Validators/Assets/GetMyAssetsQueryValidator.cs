using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetMyAssetsQueryValidator : IRequestValidator<GetMyAssetsQuery>
{
    public Task ValidateAsync(GetMyAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
