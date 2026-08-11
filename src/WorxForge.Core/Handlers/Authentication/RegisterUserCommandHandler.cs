using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
{
    public Task HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
