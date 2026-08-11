using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Validators.Authentication;

public sealed class VerifyEmailCommandValidator : IRequestValidator<VerifyEmailCommand>
{
    public Task ValidateAsync(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
