using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Handlers.Settings;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    public Task HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
