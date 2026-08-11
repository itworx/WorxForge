using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class GetAdminUsersQueryValidator : IRequestValidator<GetAdminUsersQuery>
{
    public Task ValidateAsync(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
