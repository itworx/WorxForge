using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Dtos.Authentication;
using WorxForge.Domain.Enums;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Entities;
using WorxForge.Infrastructure.Options;

namespace WorxForge.Infrastructure.Services;

public sealed class AuthenticationService(
    IUserRepository userRepository,
    IAuthSessionRepository authSessionRepository,
    IOptions<JwtAccessTokenOptions> jwtOptions,
    IConfiguration configuration) : IAuthenticationService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthSessionRepository _authSessionRepository = authSessionRepository;
    private readonly JwtAccessTokenOptions _jwtOptions = jwtOptions.Value;
    private readonly IConfiguration _configuration = configuration;
    private const string DevelopmentGoogleSubjectSecretKey = "developmentGoogleSubject";
    private const string DevelopmentEmailSecretKey = "developmentEmail";

    public async Task<AuthTokenDto> AuthenticateDevelopmentUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var developmentGoogleSubject = ResolveRequiredDevelopmentSecret(DevelopmentGoogleSubjectSecretKey);
        var developmentEmail = ResolveRequiredDevelopmentSecret(DevelopmentEmailSecretKey);

        var user = await _userRepository.GetByGoogleIdAsync(developmentGoogleSubject, cancellationToken);
        if (user is not null)
        {
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        user = await _userRepository.GetByEmailAsync(developmentEmail, cancellationToken);
        if (user is null)
        {
            var nowUtc = DateTime.UtcNow;
            var generatedUsername = await GenerateUniqueUsernameFromEmailAsync(developmentEmail, cancellationToken);
            user = new User
            {
                UserId = Guid.NewGuid(),
                Username = generatedUsername,
                DisplayName = developmentEmail,
                Email = developmentEmail,
                EmailVerified = true,
                Role = UserRole.User.ToString(),
                GoogleId = developmentGoogleSubject,
                AvatarUrl = null,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            };

            await _userRepository.AddAsync(user, cancellationToken);
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(user.GoogleId))
        {
            user.GoogleId = developmentGoogleSubject;
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        if (!string.Equals(user.GoogleId, developmentGoogleSubject, StringComparison.Ordinal))
        {
            throw new WorxForgeException("Test user email is linked to a different Google account.");
        }

        return await BuildAccessTokenAsync(user, cancellationToken);
    }

    private string ResolveRequiredDevelopmentSecret(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new WorxForgeException(
                $"Development authentication secret '{key}' is missing. Configure it using ASP.NET Core user-secrets."
            );
        }

        return value.Trim();
    }

    public Task RegisterAsync(string email, string username, string password, string passwordConfirm, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<AuthTokenDto> AuthenticateAsync(string emailOrUsername, string password, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task<AuthTokenDto> AuthenticateWithGoogleAsync(string idToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new WorxForgeException("Google ID token is required.");
        }

        var googleClientId = ResolveGoogleClientId();

        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [googleClientId]
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
        }
        catch (InvalidJwtException ex)
        {
            throw new WorxForgeException("Google ID token is invalid.", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new WorxForgeException("Failed to validate Google ID token.", ex);
        }

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new WorxForgeException("Google account email must be verified.");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new WorxForgeException("Google subject is missing from ID token payload.");
        }

        var googleSubject = payload.Subject.Trim();
        var normalizedEmail = payload.Email.Trim();
        var user = await _userRepository.GetByGoogleIdAsync(googleSubject, cancellationToken);

        if (user is not null)
        {
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            var generatedUsername = await GenerateUniqueUsernameFromEmailAsync(normalizedEmail, cancellationToken);
            user = CreateUserFromGooglePayload(payload, normalizedEmail, generatedUsername);
            await _userRepository.AddAsync(user, cancellationToken);
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(user.GoogleId))
        {
            user.GoogleId = googleSubject;
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return await BuildAccessTokenAsync(user, cancellationToken);
        }

        if (!string.Equals(user.GoogleId, googleSubject, StringComparison.Ordinal))
        {
            throw new WorxForgeException("This email is already linked to a different Google account.");
        }

        return await BuildAccessTokenAsync(user, cancellationToken);
    }
    public Task VerifyEmailAsync(string token, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ResetPasswordAsync(string token, string newPassword, string newPasswordConfirm, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            throw new WorxForgeException("User identifier is required.");
        }

        if (sessionId == Guid.Empty)
        {
            throw new WorxForgeException("Session identifier is required.");
        }

        var session = await _authSessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null || session.UserId != userId)
        {
            throw new WorxForgeException("Session was not found.");
        }

        await _authSessionRepository.RevokeAsync(sessionId, cancellationToken);
    }

    private string ResolveGoogleClientId()
    {
        var clientId = _configuration["Authentication:Google:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new WorxForgeException(
                "Google Client ID configuration is missing. Configure Authentication:Google:ClientId."
            );
        }

        return clientId;
    }

    private async Task<string> GenerateUniqueUsernameFromEmailAsync(string email, CancellationToken cancellationToken)
    {
        var baseUsername = ExtractUsernameBaseFromEmail(email);
        if (!await _userRepository.ExistsByUsernameAsync(baseUsername, cancellationToken))
        {
            return baseUsername;
        }

        const int maxAttempts = 20;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomSuffix = RandomNumberGenerator.GetInt32(10, 1000);
            var candidate = $"{baseUsername}{randomSuffix}";

            if (!await _userRepository.ExistsByUsernameAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        while (true)
        {
            var randomSuffix = RandomNumberGenerator.GetInt32(1000, 10000);
            var candidate = $"{baseUsername}{randomSuffix}";

            if (!await _userRepository.ExistsByUsernameAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }
    }

    private static string ExtractUsernameBaseFromEmail(string email)
    {
        var trimmedEmail = email.Trim();
        var atIndex = trimmedEmail.IndexOf('@');
        var localPart = atIndex > 0 ? trimmedEmail[..atIndex] : trimmedEmail;
        var normalizedLocalPart = localPart.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedLocalPart))
        {
            return "user";
        }

        var builder = new StringBuilder(normalizedLocalPart.Length);
        foreach (var character in normalizedLocalPart)
        {
            if (char.IsLetterOrDigit(character) || character is '.' or '_' or '-')
            {
                builder.Append(character);
            }
        }

        return builder.Length == 0 ? "user" : builder.ToString();
    }

    private static User CreateUserFromGooglePayload(GoogleJsonWebSignature.Payload payload, string normalizedEmail, string username)
    {
        if (string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new WorxForgeException("Google subject is missing from ID token payload.");
        }

        var displayName = string.IsNullOrWhiteSpace(payload.Name) ? username : payload.Name.Trim();
        var nowUtc = DateTime.UtcNow;

        return new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            DisplayName = displayName,
            Email = normalizedEmail,
            EmailVerified = true,
            Role = UserRole.User.ToString(),
            GoogleId = payload.Subject,
            AvatarUrl = payload.Picture,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
    }

    private async Task<AuthTokenDto> BuildAccessTokenAsync(User user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(_jwtOptions.Audience) ||
            string.IsNullOrWhiteSpace(_jwtOptions.SigningKey) ||
            _jwtOptions.ExpirationMinutes <= 0)
        {
            throw new WorxForgeException("JwtAccessToken configuration is incomplete.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var nowUtc = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var sessionId = Guid.NewGuid();
        var tokenHash = Guid.NewGuid().ToString("N");

        var session = new AuthSession
        {
            SessionId = sessionId,
            UserId = user.UserId,
            TokenHash = tokenHash,
            IssuedAt = nowUtc,
            ExpiresAt = expiresAt,
            RevokedAt = null
        };

        await _authSessionRepository.AddAsync(session, cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimConstants.UserId, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Sid, sessionId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
        }

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimConstants.Role, user.Role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthTokenDto
        {
            AccessToken = tokenString,
            ExpiresAt = expiresAt
        };
    }
}
