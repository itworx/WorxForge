using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand>
{
    public Task HandleAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
