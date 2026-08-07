using WorxForge.Domain.Dtos.Authentication;

namespace WorxForge.Core.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthTokenDto> AuthenticateDevelopmentUserAsync(CancellationToken cancellationToken);
    Task RegisterAsync(string email, string username, string password, string passwordConfirm, CancellationToken cancellationToken);
    Task<AuthTokenDto> AuthenticateAsync(string emailOrUsername, string password, CancellationToken cancellationToken);
    Task<AuthTokenDto> AuthenticateWithGoogleAsync(string idToken, CancellationToken cancellationToken);
    Task VerifyEmailAsync(string token, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
    Task ResetPasswordAsync(string token, string newPassword, string newPasswordConfirm, CancellationToken cancellationToken);
    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
}

