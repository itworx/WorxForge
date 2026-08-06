using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Authentication;

public sealed class LogoutUserCommandValidator : IRequestValidator<LogoutUserCommand>
{
    public Task ValidateAsync(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Logout request is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new WorxForgeException("User identifier is required.");
        }

        if (request.SessionId == Guid.Empty)
        {
            throw new WorxForgeException("Session identifier is required.");
        }

        return Task.CompletedTask;
    }
}
