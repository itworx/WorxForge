using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class GetRecentlyUsedAssetsQueryHandler : IQueryHandler<GetRecentlyUsedAssetsQuery, WorxForge.Domain.Dtos.Library.LibrarySummaryDto>
{
    public Task<WorxForge.Domain.Dtos.Library.LibrarySummaryDto> HandleAsync(GetRecentlyUsedAssetsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
