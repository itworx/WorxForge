using WorxForge.Domain.Dtos.Profile;

namespace WorxForge.Core.Interfaces.Services;

public interface IProfileService
{
    Task<MyProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<PublicProfileDto> GetPublicProfileAsync(Guid targetUserId, CancellationToken cancellationToken);
    Task UpdateProfileAsync(Guid userId, string displayName, string username, string? avatarUrl, CancellationToken cancellationToken);
}

