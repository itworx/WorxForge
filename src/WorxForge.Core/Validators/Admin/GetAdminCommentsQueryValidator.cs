using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class GetAdminCommentsQueryValidator : IRequestValidator<GetAdminCommentsQuery>
{
    public Task ValidateAsync(GetAdminCommentsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
