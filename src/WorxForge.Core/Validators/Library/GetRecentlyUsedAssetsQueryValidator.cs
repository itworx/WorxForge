using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Validators.Library;

public sealed class GetRecentlyUsedAssetsQueryValidator : IRequestValidator<GetRecentlyUsedAssetsQuery>
{
    public Task ValidateAsync(GetRecentlyUsedAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
