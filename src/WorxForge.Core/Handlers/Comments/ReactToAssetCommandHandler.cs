using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Comments;
using WorxForge.Domain.Dtos.Assets;

namespace WorxForge.Core.Handlers.Comments;

public sealed class ReactToAssetCommandHandler(IReactionService reactionService) : ICommandHandler<ReactToAssetCommand, AssetReactionDto>
{
    public Task<AssetReactionDto> HandleAsync(ReactToAssetCommand command, CancellationToken cancellationToken) =>
    reactionService.ReactToAssetAsync(command.UserId, command.AssetId, command.ReactionType, cancellationToken);
}
