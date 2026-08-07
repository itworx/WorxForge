using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Dtos.Explore;
using WorxForge.Domain.Queries.Lookup;

namespace WorxForge.Core.Handlers.Lookup;

public sealed class GetToolsQueryHandler(
    IToolRepository toolRepository) : IQueryHandler<GetToolsQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<IReadOnlyList<LookupItemDto>> HandleAsync(GetToolsQuery query, CancellationToken cancellationToken)
    {
        var tools = await toolRepository.GetAllAsync(cancellationToken);
        return tools.Select(x => new LookupItemDto { Id = x.ToolId, Name = x.Name }).ToList();
    }
}
