namespace WorxForge.Domain.Queries.Notifications;

public sealed class GetNotificationsQuery
{
    public Guid UserId { get; init; }
    public bool? IsRead { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

