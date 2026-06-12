namespace IdentitySerivce.Domain.Entity;

public class AnalyticsEvent
{
    public long Id { get; set; }
    public string EventType { get; set; } = "page_view";
    public string Path { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string VisitorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
