using WorxForge.Domain.Entities;

namespace WorxForge.Core.Interfaces.Repositories;

// TODO: AUTH_SESSION lifecycle details pending decision.
public interface IAuthSessionRepository
{
    Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<bool> IsValidAsync(string tokenHash, CancellationToken cancellationToken);
    Task AddAsync(AuthSession session, CancellationToken cancellationToken);
    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}

