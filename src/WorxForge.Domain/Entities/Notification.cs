namespace WorxForge.Domain.Entities;

public sealed class Notification
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? AssetId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? ReportId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
