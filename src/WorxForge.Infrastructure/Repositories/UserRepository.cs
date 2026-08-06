using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class UserRepository(WorxForgeDbContext dbContext) : IUserRepository
{
    private readonly WorxForgeDbContext _dbContext = dbContext;

    private IQueryable<User> UsersForRead() =>
        _dbContext.Users
            .FromSqlRaw(
                @"SELECT
                    [user_id],
                    [username],
                    [display_name],
                    [email],
                    [email_verified],
                    [role],
                    CAST([google_id] AS nvarchar(450)) AS [google_id],
                    [avatar_url],
                    [created_at],
                    [updated_at],
                    [deleted_at]
                FROM [USER]")
            .AsNoTracking();

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return UsersForRead()
            .FirstOrDefaultAsync(user => user.UserId == userId && user.DeletedAt == null, cancellationToken);
    }
    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken)
    {
        return UsersForRead()
            .FirstOrDefaultAsync(user => user.GoogleId == googleId && user.DeletedAt == null, cancellationToken);
    }
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return UsersForRead()
            .FirstOrDefaultAsync(user => user.Email == email && user.DeletedAt == null, cancellationToken);
    }
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return UsersForRead()
            .FirstOrDefaultAsync(user => user.Username == username && user.DeletedAt == null, cancellationToken);
    }
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AnyAsync(user => user.Email == email && user.DeletedAt == null, cancellationToken);
    }
    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AnyAsync(user => user.Username == username && user.DeletedAt == null, cancellationToken);
    }
    public Task<IReadOnlyList<User>> GetAllAsync(string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public Task SoftDeleteAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
