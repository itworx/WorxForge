using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Validators.Explore;

public sealed class SearchAssetsQueryValidator : IRequestValidator<SearchAssetsQuery>
{
    public Task ValidateAsync(SearchAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
