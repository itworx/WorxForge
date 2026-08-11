using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class GetFollowingAssetsQueryHandler : IQueryHandler<GetFollowingAssetsQuery, WorxForge.Domain.Dtos.Library.LibrarySummaryDto>
{
    public Task<WorxForge.Domain.Dtos.Library.LibrarySummaryDto> HandleAsync(GetFollowingAssetsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
