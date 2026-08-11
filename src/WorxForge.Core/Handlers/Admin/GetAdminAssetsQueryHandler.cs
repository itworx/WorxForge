using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class GetAdminAssetsQueryHandler : IQueryHandler<GetAdminAssetsQuery, WorxForge.Domain.Dtos.Admin.AdminAssetDto>
{
    public Task<WorxForge.Domain.Dtos.Admin.AdminAssetDto> HandleAsync(GetAdminAssetsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
