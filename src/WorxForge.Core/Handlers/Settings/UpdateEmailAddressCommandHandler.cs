using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Settings;

namespace WorxForge.Core.Handlers.Settings;

public sealed class UpdateEmailAddressCommandHandler : ICommandHandler<UpdateEmailAddressCommand>
{
    public Task HandleAsync(UpdateEmailAddressCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
