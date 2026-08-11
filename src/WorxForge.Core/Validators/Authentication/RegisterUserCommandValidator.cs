using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Validators.Authentication;

public sealed class RegisterUserCommandValidator : IRequestValidator<RegisterUserCommand>
{
    public Task ValidateAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
