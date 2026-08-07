using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

/// <summary>Runs all <see cref="IDataSeeder"/> implementations against the database on startup.</summary>
public static class DataSeeder
{
    private static readonly IDataSeeder[] Seeders =
    [
        new AssetTypeSeeder(),
        new BusinessDomainSeeder(),
        new UseCaseSeeder(),
        new ToolSeeder()
    ];

    public static async Task SeedWorxForgeDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorxForgeDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDataSeeder>>();

        foreach (var seeder in Seeders)
        {
            var seederName = seeder.GetType().Name;
            logger.LogInformation("Running {Seeder}", seederName);

            try
            {
                var insertedCount = await seeder.SeedAsync(context, cancellationToken);
                logger.LogInformation("{Seeder} completed, inserted {InsertedCount} row(s)", seederName, insertedCount);
            }
            catch (Exception ex)
            {
                // Seeding is best-effort; a failure here must not prevent the app from starting.
                logger.LogError(ex, "{Seeder} failed to seed lookup data", seederName);
            }
        }
    }
}
