using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Validators.Settings;

public sealed class UpdateEmailAddressCommandValidator : IRequestValidator<UpdateEmailAddressCommand>
{
    public Task ValidateAsync(UpdateEmailAddressCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
