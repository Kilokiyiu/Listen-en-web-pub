namespace IdentitySerivce.Domain.Entity;

public class AnalyticsDaily
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public string Path { get; set; } = string.Empty;
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }
}
