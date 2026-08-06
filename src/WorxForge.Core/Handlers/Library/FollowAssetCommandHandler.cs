using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class FollowAssetCommandHandler(ILibraryService libraryService) : ICommandHandler<FollowAssetCommand>
{
    public Task HandleAsync(FollowAssetCommand command, CancellationToken cancellationToken) =>
        libraryService.FollowAssetAsync(command.UserId, command.AssetId, cancellationToken);
}
