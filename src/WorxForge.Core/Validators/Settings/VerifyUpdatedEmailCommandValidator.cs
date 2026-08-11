using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Validators.Settings;

public sealed class VerifyUpdatedEmailCommandValidator : IRequestValidator<VerifyUpdatedEmailCommand>
{
    public Task ValidateAsync(VerifyUpdatedEmailCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
