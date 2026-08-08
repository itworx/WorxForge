using WorxForge.API.Options;

namespace WorxForge.API.Configuration;

public static class CorsConfigurationExtensions
{
    public const string PolicyName = "WorxForgeCors";

    public static IServiceCollection AddWorxForgeCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [];

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException(
                "Cors configuration is incomplete. Cors:AllowedOrigins must contain at least one origin.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }
}
