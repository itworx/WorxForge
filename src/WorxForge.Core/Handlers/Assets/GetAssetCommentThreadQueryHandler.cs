using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Queries.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class GetAssetCommentThreadQueryHandler(
    IRequestValidator<GetAssetCommentThreadQuery> validator,
    ICommentService commentService) : IQueryHandler<GetAssetCommentThreadQuery, AssetCommentDto>
{
    public async Task<AssetCommentDto> HandleAsync(GetAssetCommentThreadQuery query, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(query, cancellationToken);
        return await commentService.GetAssetCommentThreadAsync(query.AssetId, query.CommentId, cancellationToken);
    }
}