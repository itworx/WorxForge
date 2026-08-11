using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class PreviewAssetCommandHandler : ICommandHandler<PreviewAssetCommand>
{
    public Task HandleAsync(PreviewAssetCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
