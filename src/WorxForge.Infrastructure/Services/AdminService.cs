using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Admin;
using WorxForge.Domain.Dtos.Common;
using WorxForge.Domain.Enums;

namespace WorxForge.Infrastructure.Services;

public sealed class AdminService : IAdminService
{
    public Task<PagedResultDto<AdminUserDto>> GetUsersAsync(string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Guid> CreateUserAsync(string email, string username, string displayName, UserRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<PagedResultDto<AdminAssetDto>> GetAssetsAsync(string? search, string? status, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AdminCommentDto>> GetCommentsAsync(int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResultDto<AdminReportDto>> GetReportsAsync(ReportStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ResolveReportAsync(Guid reportId, Guid adminUserId, ReportStatus newStatus, CancellationToken cancellationToken) => throw new NotImplementedException();
}
