using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class UpdateAssetCommandHandler(
    IRequestValidator<UpdateAssetCommand> validator,
    IAssetService assetService) : ICommandHandler<UpdateAssetCommand>
{
    public async Task HandleAsync(UpdateAssetCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        await assetService.UpdateAsync(command, cancellationToken);
    }
}
