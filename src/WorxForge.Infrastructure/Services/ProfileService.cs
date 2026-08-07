using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Infrastructure.Services;

public sealed class ProfileService(IUserRepository userRepository) : IProfileService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new WorxForgeException("User was not found.");
        }

        return new ProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<ProfileDto> GetPublicProfileAsync(Guid targetUserId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
        {
            throw new WorxForgeException("User was not found.");
        }

        return new ProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateProfileAsync(Guid userId, string displayName, string username, string? avatarUrl, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new WorxForgeException("User was not found.");
        }

        var normalizedDisplayName = displayName.Trim();
        var normalizedUsername = username.Trim();
        var normalizedAvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();

        var existingByUsername = await _userRepository.GetByUsernameAsync(normalizedUsername, cancellationToken);
        if (existingByUsername is not null && existingByUsername.UserId != userId)
        {
            throw new WorxForgeException("Username is already in use.");
        }

        user.DisplayName = normalizedDisplayName;
        user.Username = normalizedUsername;
        user.AvatarUrl = normalizedAvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
