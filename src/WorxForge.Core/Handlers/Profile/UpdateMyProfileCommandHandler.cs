using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Profile;

namespace WorxForge.Core.Handlers.Profile;

public sealed class UpdateMyProfileCommandHandler : ICommandHandler<UpdateMyProfileCommand>
{
    private readonly IRequestValidator<UpdateMyProfileCommand> _validator;
    private readonly IProfileService _profileService;

    public UpdateMyProfileCommandHandler(
        IRequestValidator<UpdateMyProfileCommand> validator,
        IProfileService profileService)
    {
        _validator = validator;
        _profileService = profileService;
    }

    public async Task HandleAsync(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(command, cancellationToken);

        await _profileService.UpdateProfileAsync(
            command.UserId,
            command.DisplayName,
            command.Username,
            command.AvatarUrl,
            cancellationToken);
    }
}
