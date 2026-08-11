using WorxForge.Domain.Constants;

namespace WorxForge.API.Configuration;

public static class AuthorizationConfigurationExtensions
{
    public static IServiceCollection AddWorxForgeAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyConstants.AuthenticatedUser, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(PolicyConstants.AdminOnly, policy => policy.RequireRole("Admin"));

        return services;
    }
}
