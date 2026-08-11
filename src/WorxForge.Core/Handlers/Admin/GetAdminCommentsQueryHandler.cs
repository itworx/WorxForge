using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class GetAdminCommentsQueryHandler : IQueryHandler<GetAdminCommentsQuery, WorxForge.Domain.Dtos.Admin.AdminCommentDto>
{
    public Task<WorxForge.Domain.Dtos.Admin.AdminCommentDto> HandleAsync(GetAdminCommentsQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
