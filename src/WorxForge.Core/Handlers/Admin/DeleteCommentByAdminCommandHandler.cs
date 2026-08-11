using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class DeleteCommentByAdminCommandHandler : ICommandHandler<DeleteCommentByAdminCommand>
{
    public Task HandleAsync(DeleteCommentByAdminCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
