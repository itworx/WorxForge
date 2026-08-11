using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Enums;

namespace WorxForge.Core.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResultDto<AdminUserDto>> GetUsersAsync(string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken);
    Task<Guid> CreateUserAsync(string email, string username, string displayName, UserRole role, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<AdminAssetDto>> GetAssetsAsync(string? search, string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken);
    Task<PagedResultDto<AdminCommentDto>> GetCommentsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken);
    Task<PagedResultDto<AdminReportDto>> GetReportsAsync(ReportStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken);
    Task ResolveReportAsync(Guid reportId, Guid adminUserId, ReportStatus newStatus, CancellationToken cancellationToken);
}

