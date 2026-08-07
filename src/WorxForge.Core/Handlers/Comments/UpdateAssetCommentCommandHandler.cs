using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Comments;

namespace WorxForge.Core.Handlers.Comments;

public sealed class UpdateAssetCommentCommandHandler(
    IRequestValidator<UpdateAssetCommentCommand> validator,
    ICommentService commentService) : ICommandHandler<UpdateAssetCommentCommand>
{
    public async Task HandleAsync(UpdateAssetCommentCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        await commentService.UpdateCommentAsync(command.AssetId, command.CommentId, command.RequestingUserId, command.Content, cancellationToken);
    }
}