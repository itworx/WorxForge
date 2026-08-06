using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class UnfollowAssetCommandHandler(ILibraryService libraryService) : ICommandHandler<UnfollowAssetCommand>
{
    public Task HandleAsync(UnfollowAssetCommand command, CancellationToken cancellationToken) =>
        libraryService.UnfollowAssetAsync(command.UserId, command.AssetId, cancellationToken);
}
