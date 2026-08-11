using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorxForge.Domain.Entities;
using WorxForge.Domain.Enums;

namespace WorxForge.Infrastructure.Configuration;

public sealed class WorxForgeDbContext(DbContextOptions<WorxForgeDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();
    public DbSet<BusinessDomain> BusinessDomains => Set<BusinessDomain>();
    public DbSet<UseCase> UseCases => Set<UseCase>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<AssetTool> AssetTools => Set<AssetTool>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<AssetTag> AssetTags => Set<AssetTag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<AssetView> AssetViews => Set<AssetView>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportAsset> ReportAssets => Set<ReportAsset>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // SQL Server's datetime2 has no timezone info, so values come back with Kind=Unspecified;
    // re-tag them as UTC (all writes use DateTime.UtcNow) so JSON serialization emits the trailing "Z".
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder.Entity<User>());
        ConfigureAuthSession(modelBuilder.Entity<AuthSession>());
        ConfigureAssetType(modelBuilder.Entity<AssetType>());
        ConfigureBusinessDomain(modelBuilder.Entity<BusinessDomain>());
        ConfigureUseCase(modelBuilder.Entity<UseCase>());
        ConfigureAsset(modelBuilder.Entity<Asset>());
        ConfigureTool(modelBuilder.Entity<Tool>());
        ConfigureAssetTool(modelBuilder.Entity<AssetTool>());
        ConfigureTag(modelBuilder.Entity<Tag>());
        ConfigureAssetTag(modelBuilder.Entity<AssetTag>());
        ConfigureComment(modelBuilder.Entity<Comment>());
        ConfigureReaction(modelBuilder.Entity<Reaction>());
        ConfigureAssetView(modelBuilder.Entity<AssetView>());
        ConfigureBookmark(modelBuilder.Entity<Bookmark>());
        ConfigureReport(modelBuilder.Entity<Report>());
        ConfigureReportAsset(modelBuilder.Entity<ReportAsset>());
        ConfigureNotification(modelBuilder.Entity<Notification>());
    }

    private static void ConfigureUser(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("USER");
        entity.HasKey(e => e.UserId);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAuthSession(EntityTypeBuilder<AuthSession> entity)
    {
        entity.ToTable("AUTH_SESSION");
        entity.HasKey(e => e.SessionId);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAssetType(EntityTypeBuilder<AssetType> entity)
    {
        entity.ToTable("ASSET_TYPE");
        entity.HasKey(e => e.AssetTypeId);
        entity.HasIndex(e => e.Name).IsUnique();
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureBusinessDomain(EntityTypeBuilder<BusinessDomain> entity)
    {
        entity.ToTable("DOMAIN");
        entity.HasKey(e => e.DomainId);
        entity.HasIndex(e => e.Name).IsUnique();
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureUseCase(EntityTypeBuilder<UseCase> entity)
    {
        entity.ToTable("USE_CASE");
        entity.HasKey(e => e.UseCaseId);
        entity.HasIndex(e => e.Name).IsUnique();
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAsset(EntityTypeBuilder<Asset> entity)
    {
        entity.ToTable("ASSET");
        entity.HasKey(e => e.AssetId);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<AssetType>()
            .WithMany()
            .HasForeignKey(e => e.AssetTypeId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<BusinessDomain>()
            .WithMany()
            .HasForeignKey(e => e.DomainId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<UseCase>()
            .WithMany()
            .HasForeignKey(e => e.UseCaseId)
            .OnDelete(DeleteBehavior.NoAction);
        // Bounded so it can be indexed (SQL Server index keys are capped at 900 bytes); speeds up the
        // LIKE-based title search's exact-match scoring and title-sorted lookups.
        entity.Property(e => e.Title).HasMaxLength(300);
        entity.HasIndex(e => e.Title);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureTool(EntityTypeBuilder<Tool> entity)
    {
        entity.ToTable("TOOL");
        entity.HasKey(e => e.ToolId);
        entity.HasIndex(e => e.Name).IsUnique();
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAssetTool(EntityTypeBuilder<AssetTool> entity)
    {
        entity.ToTable("ASSET_TOOL");
        entity.HasKey(e => new { e.AssetId, e.ToolId });
        entity.HasOne<Asset>()
            .WithMany(asset => asset.AssetTools)
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Tool>()
            .WithMany()
            .HasForeignKey(e => e.ToolId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureTag(EntityTypeBuilder<Tag> entity)
    {
        entity.ToTable("TAG");
        entity.HasKey(e => e.TagId);
        entity.HasIndex(e => e.Name).IsUnique();
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAssetTag(EntityTypeBuilder<AssetTag> entity)
    {
        entity.ToTable("ASSET_TAG");
        entity.HasKey(e => new { e.AssetId, e.TagId });
        entity.HasOne<Asset>()
            .WithMany(asset => asset.AssetTags)
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureComment(EntityTypeBuilder<Comment> entity)
    {
        entity.ToTable("COMMENT");
        entity.HasKey(e => e.CommentId);
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(e => e.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureReaction(EntityTypeBuilder<Reaction> entity)
    {
        entity.ToTable("REACTION");
        entity.HasKey(e => new { e.AssetId, e.UserId });
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureAssetView(EntityTypeBuilder<AssetView> entity)
    {
        entity.ToTable("ASSET_VIEW");
        entity.HasKey(e => new { e.UserId, e.AssetId });
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureBookmark(EntityTypeBuilder<Bookmark> entity)
    {
        entity.ToTable("BOOKMARK");
        entity.HasKey(e => new { e.UserId, e.AssetId });
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureReport(EntityTypeBuilder<Report> entity)
    {
        entity.ToTable("REPORT");
        entity.HasKey(e => e.ReportId);
        entity.Property(e => e.Status).HasConversion<string>();
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ReporterUserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ResolvedBy)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureReportAsset(EntityTypeBuilder<ReportAsset> entity)
    {
        entity.ToTable("REPORT_ASSET");
        entity.HasKey(e => e.ReportId);
        entity.HasOne<Report>()
            .WithOne()
            .HasForeignKey<ReportAsset>(e => e.ReportId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ConfigureNotification(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("NOTIFICATION");
        entity.HasKey(e => e.NotificationId);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.RecipientUserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<Report>()
            .WithMany()
            .HasForeignKey(e => e.ReportId)
            .OnDelete(DeleteBehavior.NoAction);
        ApplySnakeCaseColumnNames(entity);
    }

    private static void ApplySnakeCaseColumnNames<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        foreach (var property in entity.Metadata.GetProperties())
        {
            entity.Property(property.Name).HasColumnName(ToSnakeCase(property.Name));
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var characters = new List<char>(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (i > 0 && char.IsUpper(current))
            {
                var previous = value[i - 1];
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);

                if (char.IsLower(previous) || char.IsDigit(previous) || nextIsLower)
                {
                    characters.Add('_');
                }
            }

            characters.Add(char.ToLowerInvariant(current));
        }

        return new string(characters.ToArray());
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private sealed class UtcNullableDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
        v => v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
}
