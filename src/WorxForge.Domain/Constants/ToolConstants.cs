namespace WorxForge.Domain.Constants;

// Fixed lookup rows so foreign keys and seed data stay stable across environments and re-seeds.
public static class ToolConstants
{
    public static readonly LookupEntry WorxGpt = new(new Guid("41111111-1111-1111-1111-111111111101"), "WorxGPT");
    public static readonly LookupEntry ClaudeCode = new(new Guid("41111111-1111-1111-1111-111111111102"), "Claude Code");
    public static readonly LookupEntry GitHubCopilot = new(new Guid("41111111-1111-1111-1111-111111111103"), "GitHub Copilot");
    public static readonly LookupEntry OpenAi = new(new Guid("41111111-1111-1111-1111-111111111104"), "OpenAI");
    public static readonly LookupEntry Anthropic = new(new Guid("41111111-1111-1111-1111-111111111105"), "Anthropic");
    public static readonly LookupEntry ToolAgnostic = new(new Guid("41111111-1111-1111-1111-111111111106"), "Tool agnostic");

    public static readonly IReadOnlyList<LookupEntry> All =
    [
        WorxGpt, ClaudeCode, GitHubCopilot, OpenAi, Anthropic, ToolAgnostic
    ];
}
