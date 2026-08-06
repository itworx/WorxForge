namespace WorxForge.Domain.Constants;

// Fixed lookup rows so foreign keys and seed data stay stable across environments and re-seeds.
public static class UseCaseConstants
{
    public static readonly LookupEntry CodeGeneration = new(new Guid("31111111-1111-1111-1111-111111111101"), "Code generation");
    public static readonly LookupEntry CodeReview = new(new Guid("31111111-1111-1111-1111-111111111102"), "Code review");
    public static readonly LookupEntry Testing = new(new Guid("31111111-1111-1111-1111-111111111103"), "Testing");
    public static readonly LookupEntry Requirements = new(new Guid("31111111-1111-1111-1111-111111111104"), "Requirements");
    public static readonly LookupEntry Documentation = new(new Guid("31111111-1111-1111-1111-111111111105"), "Documentation");
    public static readonly LookupEntry Debugging = new(new Guid("31111111-1111-1111-1111-111111111106"), "Debugging");
    public static readonly LookupEntry ArchitectureDesign = new(new Guid("31111111-1111-1111-1111-111111111107"), "Architecture design");
    public static readonly LookupEntry SecurityReview = new(new Guid("31111111-1111-1111-1111-111111111108"), "Security review");
    public static readonly LookupEntry RepositoryOnboarding = new(new Guid("31111111-1111-1111-1111-111111111109"), "Repository onboarding");
    public static readonly LookupEntry GeneralAssistance = new(new Guid("31111111-1111-1111-1111-111111111110"), "General Assistance");

    public static readonly IReadOnlyList<LookupEntry> All =
    [
        CodeGeneration, CodeReview, Testing, Requirements, Documentation,
        Debugging, ArchitectureDesign, SecurityReview, RepositoryOnboarding, GeneralAssistance
    ];
}
