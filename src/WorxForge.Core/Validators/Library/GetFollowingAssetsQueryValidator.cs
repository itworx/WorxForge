using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Validators.Library;

public sealed class GetFollowingAssetsQueryValidator : IRequestValidator<GetFollowingAssetsQuery>
{
    public Task ValidateAsync(GetFollowingAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
