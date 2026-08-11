using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Validators.Authentication;

public sealed class RequestPasswordResetCommandValidator : IRequestValidator<RequestPasswordResetCommand>
{
    public Task ValidateAsync(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
