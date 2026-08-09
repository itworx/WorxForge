using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class RemoveBookmarkCommandHandler(ILibraryService libraryService) : ICommandHandler<RemoveBookmarkCommand>
{
    public Task HandleAsync(RemoveBookmarkCommand command, CancellationToken cancellationToken) =>
        libraryService.RemoveBookmarkAsync(command.UserId, command.AssetId, cancellationToken);
}
