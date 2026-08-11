using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class DeleteAssetCommandHandler(
    IRequestValidator<DeleteAssetCommand> validator,
    IAssetService assetService) : ICommandHandler<DeleteAssetCommand>
{
    public async Task HandleAsync(DeleteAssetCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        await assetService.DeleteAsync(command.AssetId, command.RequestingUserId, cancellationToken);
    }
}
