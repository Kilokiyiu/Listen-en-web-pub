namespace IdentitySerivce.Domain.Entity;

/// <summary>
/// 用户学习活动（听力完成 / 文章阅读等），用于学习记录与个人中心统计。
/// </summary>
public class StudyActivity
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>listen | article</summary>
    public string ActivityType { get; set; } = string.Empty;
    /// <summary>albumId 或 articleId</summary>
    public string ContentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    /// <summary>CET-4 / CET-6 / daily 等</summary>
    public string Category { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
