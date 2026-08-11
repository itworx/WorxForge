using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    public Task HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
