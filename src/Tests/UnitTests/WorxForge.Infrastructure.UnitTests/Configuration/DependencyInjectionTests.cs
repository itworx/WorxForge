using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorxForge.Core.Interfaces.Repositories;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Infrastructure.Configuration;

namespace WorxForge.Infrastructure.UnitTests;

public sealed class DependencyInjectionTests
{
    private static IServiceProvider BuildProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=TestDb;Trusted_Connection=True;"
            })
            .Build();

        return new ServiceCollection()
            .AddWorxForgeInfrastructure(config)
            .BuildServiceProvider();
    }

    [Fact]
    public void AddWorxForgeInfrastructure_ShouldRegisterAssetService()
    {
        var provider = BuildProvider();
        provider.GetService<IAssetService>().Should().NotBeNull();
    }

    [Fact]
    public void AddWorxForgeInfrastructure_ShouldRegisterAssetRepository()
    {
        var provider = BuildProvider();
        provider.GetService<IAssetRepository>().Should().NotBeNull();
    }
}

