using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Validators.Authentication;

public sealed class ResetPasswordCommandValidator : IRequestValidator<ResetPasswordCommand>
{
    public Task ValidateAsync(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
