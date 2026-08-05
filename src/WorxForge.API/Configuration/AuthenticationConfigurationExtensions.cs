using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Domain.Constants;
using WorxForge.Infrastructure.Options;

namespace WorxForge.API.Configuration;

public static class AuthenticationConfigurationExtensions
{
    public static IServiceCollection AddJwtAccessTokenAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtAccessTokenOptions>>((options, jwtOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;

                if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
                    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
                    string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
                {
                    throw new InvalidOperationException(
                        "JwtAccessToken configuration is incomplete. Issuer, Audience, and SigningKey are required.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.MapInboundClaims = false;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var authSessionRepository = context.HttpContext.RequestServices.GetRequiredService<IAuthSessionRepository>();
                        var userIdClaim = context.Principal?.FindFirst(ClaimConstants.UserId)?.Value;
                        var sessionIdClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

                        if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(sessionIdClaim, out var sessionId))
                        {
                            context.Fail("Token session claims are invalid.");
                            return;
                        }

                        var session = await authSessionRepository.GetByIdAsync(sessionId, context.HttpContext.RequestAborted);
                        if (session is null)
                        {
                            context.Fail("Session was not found.");
                            return;
                        }

                        if (session.UserId != userId)
                        {
                            context.Fail("Session user mismatch.");
                            return;
                        }

                        if (session.RevokedAt is not null)
                        {
                            context.Fail("Session is revoked.");
                            return;
                        }

                        if (session.ExpiresAt <= DateTime.UtcNow)
                        {
                            context.Fail("Session is expired.");
                            return;
                        }
                    }
                };
            });

        return services;
    }
}
