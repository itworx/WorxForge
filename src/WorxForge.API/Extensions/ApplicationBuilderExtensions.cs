using WorxForge.API.EndpointGroups;
using WorxForge.API.Hubs;
using WorxForge.API.Middlewares;
using WorxForge.Domain.Constants;

namespace WorxForge.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseWorxForgePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        // Enforce TLS so the SignalR "access_token" query string (see
        // AuthenticationConfigurationExtensions.OnMessageReceived) is never sent in cleartext.
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapWorxForgeEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapProfileEndpoints();
        app.MapSettingsEndpoints();
        app.MapHomeEndpoints();
        app.MapExploreEndpoints();
        app.MapAssetsEndpoints();
        app.MapLibraryEndpoints();
        app.MapNotificationsEndpoints();
        app.MapAdminEndpoints();
        app.MapHub<NotificationHub>("/hubs/notifications").RequireAuthorization(PolicyConstants.AuthenticatedUser);
        return app;
    }
}
