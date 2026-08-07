using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Comments;

namespace WorxForge.Core.Handlers.Comments;

public sealed class CreateAssetCommentCommandHandler(
    IRequestValidator<CreateAssetCommentCommand> validator,
    ICommentService commentService) : ICommandHandler<CreateAssetCommentCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateAssetCommentCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        return await commentService.CreateCommentAsync(command.UserId, command.AssetId, command.ParentCommentId, command.Content, cancellationToken);
    }
}
