namespace WorxForge.Infrastructure.Options;

public sealed class PaginationOptions
{
    public const string SectionName = "Pagination";

    public int DefaultPageSize { get; set; }
    public int MaxPageSize { get; set; }
}
