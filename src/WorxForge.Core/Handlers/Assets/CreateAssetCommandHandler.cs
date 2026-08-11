using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Assets;
using WorxForge.Domain.Dtos.Assets;

namespace WorxForge.Core.Handlers.Assets;

public sealed class CreateAssetCommandHandler(
    IRequestValidator<CreateAssetCommand> validator,
    IAssetService assetService) : ICommandHandler<CreateAssetCommand, PublishAssetResponse>
{
    public async Task<PublishAssetResponse> HandleAsync(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(command, cancellationToken);
        var assetId = await assetService.CreateAsync(command, cancellationToken);
        return new PublishAssetResponse { AssetId = assetId };
    }
}
