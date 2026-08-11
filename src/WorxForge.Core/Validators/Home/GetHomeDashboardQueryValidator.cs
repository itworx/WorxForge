using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Home;

namespace WorxForge.Core.Validators.Home;

public sealed class GetHomeDashboardQueryValidator : IRequestValidator<GetHomeDashboardQuery>
{
    public Task ValidateAsync(GetHomeDashboardQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
