namespace WorxForge.Domain.Dtos.Common;

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}

