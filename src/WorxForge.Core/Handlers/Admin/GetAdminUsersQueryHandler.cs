using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class GetAdminUsersQueryHandler : IQueryHandler<GetAdminUsersQuery, WorxForge.Domain.Dtos.Admin.AdminUserDto>
{
    public Task<WorxForge.Domain.Dtos.Admin.AdminUserDto> HandleAsync(GetAdminUsersQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
