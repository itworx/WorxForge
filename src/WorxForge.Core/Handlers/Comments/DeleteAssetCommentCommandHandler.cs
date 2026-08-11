using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Comments;

namespace WorxForge.Core.Handlers.Comments;

public sealed class DeleteAssetCommentCommandHandler(
    IRequestValidator<DeleteAssetCommentCommand> validator,
    ICommentService commentService) : ICommandHandler<DeleteAssetCommentCommand>
{
    public async Task HandleAsync(DeleteAssetCommentCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        await commentService.DeleteCommentAsync(command.AssetId, command.CommentId, command.RequestingUserId, cancellationToken);
    }
}
