using WorxForge.Domain.Dtos.Profile;

namespace WorxForge.Core.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<ProfileDto> GetPublicProfileAsync(Guid targetUserId, CancellationToken cancellationToken);
    Task UpdateProfileAsync(Guid userId, string displayName, string username, string? avatarUrl, CancellationToken cancellationToken);
}

