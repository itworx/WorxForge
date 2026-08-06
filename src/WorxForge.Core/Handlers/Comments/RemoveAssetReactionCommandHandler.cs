using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Dtos.Assets;

namespace WorxForge.Core.Handlers.Comments;

public sealed class RemoveAssetReactionCommandHandler(IReactionService reactionService) : ICommandHandler<RemoveAssetReactionCommand, AssetReactionDto>
{
    public Task<AssetReactionDto> HandleAsync(RemoveAssetReactionCommand command, CancellationToken cancellationToken) =>
    reactionService.RemoveAssetReactionAsync(command.UserId, command.AssetId, cancellationToken);
}
