using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Validators.Settings;

public sealed class ChangePasswordCommandValidator : IRequestValidator<ChangePasswordCommand>
{
    public Task ValidateAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
