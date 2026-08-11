using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class DeleteAssetByAdminCommandValidator : IRequestValidator<DeleteAssetByAdminCommand>
{
    public Task ValidateAsync(DeleteAssetByAdminCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
