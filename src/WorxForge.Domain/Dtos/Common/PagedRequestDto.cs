namespace WorxForge.Domain.Dtos.Common;

public sealed class PagedRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

