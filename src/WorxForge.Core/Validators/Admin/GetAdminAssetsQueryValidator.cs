using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class GetAdminAssetsQueryValidator : IRequestValidator<GetAdminAssetsQuery>
{
    public Task ValidateAsync(GetAdminAssetsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
