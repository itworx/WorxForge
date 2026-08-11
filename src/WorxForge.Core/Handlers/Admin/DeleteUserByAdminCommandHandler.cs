using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class DeleteUserByAdminCommandHandler : ICommandHandler<DeleteUserByAdminCommand>
{
    public Task HandleAsync(DeleteUserByAdminCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
