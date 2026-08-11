using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Handlers.Settings;

public sealed class VerifyUpdatedEmailCommandHandler : ICommandHandler<VerifyUpdatedEmailCommand>
{
    public Task HandleAsync(VerifyUpdatedEmailCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
