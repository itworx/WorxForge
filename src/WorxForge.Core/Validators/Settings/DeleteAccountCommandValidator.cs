using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Validators.Settings;

public sealed class DeleteAccountCommandValidator : IRequestValidator<DeleteAccountCommand>
{
    public Task ValidateAsync(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
