using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Validators.Library;

public sealed class GetSavedAssetsQueryValidator : IRequestValidator<GetSavedAssetsQuery>
{
    public Task ValidateAsync(GetSavedAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
