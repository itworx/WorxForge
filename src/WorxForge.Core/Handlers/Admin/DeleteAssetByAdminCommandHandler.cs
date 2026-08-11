using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Handlers.Admin;

public sealed class DeleteAssetByAdminCommandHandler : ICommandHandler<DeleteAssetByAdminCommand>
{
    public Task HandleAsync(DeleteAssetByAdminCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
