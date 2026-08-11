using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Explore;

namespace WorxForge.Core.Validators.Explore;

public sealed class GetExploreFiltersQueryValidator : IRequestValidator<GetExploreFiltersQuery>
{
    public Task ValidateAsync(GetExploreFiltersQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
