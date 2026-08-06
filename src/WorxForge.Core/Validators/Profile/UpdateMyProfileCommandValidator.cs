using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Profile;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Profile;

public sealed class UpdateMyProfileCommandValidator : IRequestValidator<UpdateMyProfileCommand>
{
    public Task ValidateAsync(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Update profile command is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new WorxForgeException("User identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new WorxForgeException("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new WorxForgeException("Username is required.");
        }

        if (request.DisplayName.Length > 100)
        {
            throw new WorxForgeException("Display name cannot exceed 100 characters.");
        }

        if (request.Username.Length > 50)
        {
            throw new WorxForgeException("Username cannot exceed 50 characters.");
        }

        if (request.AvatarUrl is not null && request.AvatarUrl.Length > 2048)
        {
            throw new WorxForgeException("Avatar URL cannot exceed 2048 characters.");
        }

        return Task.CompletedTask;
    }
}
