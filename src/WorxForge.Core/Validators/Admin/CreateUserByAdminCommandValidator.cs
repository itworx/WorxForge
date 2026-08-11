using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class CreateUserByAdminCommandValidator : IRequestValidator<CreateUserByAdminCommand>
{
    public Task ValidateAsync(CreateUserByAdminCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
