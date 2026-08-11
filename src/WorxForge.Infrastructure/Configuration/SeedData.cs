using WorxForge.Domain.Entities;

namespace WorxForge.Infrastructure.Configuration;

internal static class SeedData
{
    internal static readonly AssetType[] AssetTypes =
    [
        new AssetType { AssetTypeId = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Prompt" },
        new AssetType { AssetTypeId = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Skill" },
        new AssetType { AssetTypeId = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Agent" }
    ];

    internal static readonly BusinessDomain[] Domains =
    [
        new BusinessDomain { DomainId = Guid.Parse("21111111-1111-1111-1111-111111111101"), Name = "Development" },
        new BusinessDomain { DomainId = Guid.Parse("21111111-1111-1111-1111-111111111102"), Name = "Quality Assurance" },
        new BusinessDomain { DomainId = Guid.Parse("21111111-1111-1111-1111-111111111103"), Name = "DevOps" }
    ];

    internal static readonly UseCase[] UseCases =
    [
        new UseCase { UseCaseId = Guid.Parse("31111111-1111-1111-1111-111111111101"), Name = "Code generation" },
        new UseCase { UseCaseId = Guid.Parse("31111111-1111-1111-1111-111111111102"), Name = "Code review" },
        new UseCase { UseCaseId = Guid.Parse("31111111-1111-1111-1111-111111111103"), Name = "Testing" }
    ];

    internal static readonly Tool[] Tools =
    [
        new Tool { ToolId = Guid.Parse("41111111-1111-1111-1111-111111111101"), Name = "WorxGPT" },
        new Tool { ToolId = Guid.Parse("41111111-1111-1111-1111-111111111102"), Name = "Claude Code" },
        new Tool { ToolId = Guid.Parse("41111111-1111-1111-1111-111111111103"), Name = "GitHub Copilot" }
    ];
}
