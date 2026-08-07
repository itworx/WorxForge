using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Assets;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Exceptions;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Services;

public sealed class ProfileService(IUserRepository userRepository, WorxForgeDbContext dbContext) : IProfileService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly WorxForgeDbContext _dbContext = dbContext;

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

    public async Task<PublicProfileDto> GetPublicProfileAsync(Guid targetUserId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
        {
            throw new WorxForgeException("User was not found.");
        }

        var publishedAssets = await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.AuthorId == targetUserId)
            .OrderByDescending(asset => asset.CreatedAt)
            .Take(20)
            .Select(asset => new AssetSummaryDto
            {
                AssetId = asset.AssetId,
                Title = asset.Title,
                Description = asset.Description,
                AssetTypeName = _dbContext.AssetTypes
                    .Where(assetType => assetType.AssetTypeId == asset.AssetTypeId)
                    .Select(assetType => assetType.Name)
                    .First(),
                DomainName = _dbContext.BusinessDomains
                    .Where(domain => domain.DomainId == asset.DomainId)
                    .Select(domain => domain.Name)
                    .First(),
                Tools = _dbContext.AssetTools
                    .Where(assetTool => assetTool.AssetId == asset.AssetId)
                    .Join(_dbContext.Tools, assetTool => assetTool.ToolId, tool => tool.ToolId, (assetTool, tool) => tool.Name)
                    .ToList(),
                AuthorDisplayName = user.DisplayName,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt,
                ReactionCount = _dbContext.Reactions.Count(reaction => reaction.AssetId == asset.AssetId),
                CommentCount = _dbContext.Comments.Count(comment => comment.AssetId == asset.AssetId && comment.DeletedAt == null),
                IsBookmarked = false
            })
            .ToListAsync(cancellationToken);

        return new PublicProfileDto
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            PublishedAssets = publishedAssets,
            FollowerCount = 0
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
