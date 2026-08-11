using Microsoft.EntityFrameworkCore;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Repositories;

public sealed class AuthSessionRepository(WorxForgeDbContext dbContext) : IAuthSessionRepository
{
    private readonly WorxForgeDbContext _dbContext = dbContext;

    public Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return _dbContext.AuthSessions
            .FirstOrDefaultAsync(session => session.SessionId == sessionId, cancellationToken);
    }

    public Task<bool> IsValidAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        return _dbContext.AuthSessions.AnyAsync(
            session => session.TokenHash == tokenHash &&
                       session.RevokedAt == null &&
                       session.ExpiresAt > nowUtc,
            cancellationToken);
    }

    public async Task AddAsync(AuthSession session, CancellationToken cancellationToken)
    {
        await _dbContext.AuthSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _dbContext.AuthSessions
            .FirstOrDefaultAsync(existingSession => existingSession.SessionId == sessionId, cancellationToken);

        if (session is null)
        {
            return;
        }

        if (session.RevokedAt is null)
        {
            session.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var sessions = await _dbContext.AuthSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return;
        }

        foreach (var session in sessions)
        {
            session.RevokedAt = nowUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
