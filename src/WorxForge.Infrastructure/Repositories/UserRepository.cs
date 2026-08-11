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

    public async Task<IReadOnlyList<User>> GetAllAsync(string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = BuildUserQuery(search);

        query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "username" => query.OrderBy(user => user.Username),
            "username_desc" => query.OrderByDescending(user => user.Username),
            "email" => query.OrderBy(user => user.Email),
            "email_desc" => query.OrderByDescending(user => user.Email),
            "role" => query.OrderBy(user => user.Role),
            "role_desc" => query.OrderByDescending(user => user.Role),
            "createdat" => query.OrderBy(user => user.CreatedAt),
            "createdat_desc" => query.OrderByDescending(user => user.CreatedAt),
            "deletedat" => query.OrderBy(user => user.DeletedAt),
            "deletedat_desc" => query.OrderByDescending(user => user.DeletedAt),
            _ => query.OrderByDescending(user => user.CreatedAt)
        };

        var skip = (page - 1) * pageSize;
        return await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken)
    {
        return BuildUserQuery(search).CountAsync(cancellationToken);
    }

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

    public async Task SoftDeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (user is null || user.DeletedAt is not null)
        {
            return;
        }

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<User> BuildUserQuery(string? search)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(user =>
            user.Username.Contains(term) ||
            user.Email.Contains(term) ||
            user.DisplayName.Contains(term));
    }
}
