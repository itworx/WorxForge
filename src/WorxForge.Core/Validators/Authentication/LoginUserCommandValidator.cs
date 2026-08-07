using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Authentication;

public sealed class LoginUserCommandValidator : IRequestValidator<LoginUserCommand>
{
    public Task ValidateAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Login request is required.");
        }

        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new WorxForgeException("IdToken is required.");
        }

        return Task.CompletedTask;
    }
}
