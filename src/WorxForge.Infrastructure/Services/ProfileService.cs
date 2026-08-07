using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Infrastructure.Services;

public sealed class ProfileService(IUserRepository userRepository) : IProfileService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<MyProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new WorxForgeException("User was not found.");
        }

        return new MyProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }
    public Task<PublicProfileDto> GetPublicProfileAsync(Guid targetUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProfileStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task UpdateProfileAsync(Guid userId, string displayName, string username, string? avatarUrl, CancellationToken cancellationToken) => throw new NotImplementedException();
}
