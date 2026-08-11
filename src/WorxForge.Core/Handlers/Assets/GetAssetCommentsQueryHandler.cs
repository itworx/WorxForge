using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class GetAssetCommentsQueryHandler(
    IRequestValidator<GetAssetCommentsQuery> validator,
    ICommentService commentService) : IQueryHandler<GetAssetCommentsQuery, PagedResultDto<AssetCommentDto>>
{
    public async Task<PagedResultDto<AssetCommentDto>> HandleAsync(GetAssetCommentsQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(query, cancellationToken);
        return await commentService.GetAssetCommentsAsync(query.AssetId, query.Page, query.PageSize, cancellationToken);
    }
}
