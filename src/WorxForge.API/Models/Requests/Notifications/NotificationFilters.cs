namespace WorxForge.API.Models.Requests.Notifications;

// Client-bindable filters only; UserId always comes from the authenticated principal, never the client.
public sealed class NotificationFilters
{
    public bool? IsRead { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
