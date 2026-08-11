using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Library;

namespace WorxForge.Core.Handlers.Library;

public sealed class BookmarkAssetCommandHandler(ILibraryService libraryService) : ICommandHandler<BookmarkAssetCommand>
{
    public Task HandleAsync(BookmarkAssetCommand command, CancellationToken cancellationToken) =>
        libraryService.BookmarkAsync(command.UserId, command.AssetId, cancellationToken);
}
