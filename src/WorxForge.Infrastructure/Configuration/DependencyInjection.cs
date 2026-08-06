using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Infrastructure.Mapping.Profiles;
using WorxForge.Infrastructure.Options;
using WorxForge.Infrastructure.Repositories;
using WorxForge.Infrastructure.Services;

namespace WorxForge.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddWorxForgeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorxForgeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddAutoMapper(typeof(DomainMappingProfile).Assembly);

        services.Configure<JwtAccessTokenOptions>(configuration.GetSection(JwtAccessTokenOptions.SectionName));
        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<PaginationOptions>(configuration.GetSection(PaginationOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IAssetFollowRepository, AssetFollowRepository>();
        services.AddScoped<IReactionRepository, ReactionRepository>();
        services.AddScoped<IAssetViewRepository, AssetViewRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
        services.AddScoped<IBusinessDomainRepository, BusinessDomainRepository>();
        services.AddScoped<IUseCaseRepository, UseCaseRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReactionService, ReactionService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services;
    }
}
