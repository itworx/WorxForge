using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class DeleteUserByAdminCommandValidator : IRequestValidator<DeleteUserByAdminCommand>
{
    public Task ValidateAsync(DeleteUserByAdminCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
