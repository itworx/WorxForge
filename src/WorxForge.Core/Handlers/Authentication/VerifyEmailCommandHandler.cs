using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand>
{
    public Task HandleAsync(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
