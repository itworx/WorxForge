using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class DeleteCommentByAdminCommandValidator : IRequestValidator<DeleteCommentByAdminCommand>
{
    public Task ValidateAsync(DeleteCommentByAdminCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
