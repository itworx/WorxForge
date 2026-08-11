using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class CreateUserByAdminCommandHandler : ICommandHandler<CreateUserByAdminCommand>
{
    public Task HandleAsync(CreateUserByAdminCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
