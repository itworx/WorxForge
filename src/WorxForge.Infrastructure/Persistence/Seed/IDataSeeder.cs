using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.Persistence.Seed;

/// <summary>Seeds a set of predefined lookup values into the database.</summary>
public interface IDataSeeder
{
    /// <returns>The number of rows inserted.</returns>
    Task<int> SeedAsync(WorxForgeDbContext context, CancellationToken cancellationToken = default);
}
