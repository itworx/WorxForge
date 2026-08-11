using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;

namespace WorxForge.Core.Validators.Assets;

public sealed class PreviewAssetCommandValidator : IRequestValidator<PreviewAssetCommand>
{
    public Task ValidateAsync(PreviewAssetCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
