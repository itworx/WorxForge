using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Validators.Explore;

public sealed class BrowseAssetsQueryValidator : IRequestValidator<BrowseAssetsQuery>
{
    public Task ValidateAsync(BrowseAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
