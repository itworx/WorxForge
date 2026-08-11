namespace WorxForge.Domain.Constants;

// Fixed lookup rows so foreign keys and seed data stay stable across environments and re-seeds.
public static class AssetTypeConstants
{
    public static readonly LookupEntry Prompt = new(new Guid("11111111-1111-1111-1111-111111111101"), "Prompt");
    public static readonly LookupEntry Skill = new(new Guid("11111111-1111-1111-1111-111111111102"), "Skill");
    public static readonly LookupEntry Agent = new(new Guid("11111111-1111-1111-1111-111111111103"), "Agent");
    public static readonly LookupEntry AgentInstruction = new(new Guid("11111111-1111-1111-1111-111111111104"), "Agent instruction");
    public static readonly LookupEntry Workflow = new(new Guid("11111111-1111-1111-1111-111111111105"), "Workflow");
    public static readonly LookupEntry CodingStandard = new(new Guid("11111111-1111-1111-1111-111111111106"), "Coding standard");
    public static readonly LookupEntry Template = new(new Guid("11111111-1111-1111-1111-111111111107"), "Template");
    public static readonly LookupEntry Other = new(new Guid("11111111-1111-1111-1111-111111111108"), "Other");

    public static readonly IReadOnlyList<LookupEntry> All =
    [
        Prompt, Skill, Agent, AgentInstruction, Workflow, CodingStandard, Template, Other
    ];
}
