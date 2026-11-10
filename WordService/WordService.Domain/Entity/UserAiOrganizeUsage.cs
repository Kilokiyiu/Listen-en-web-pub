namespace WordService.Domain.Entity;

/// <summary>
/// 用户 AI 整理导入配额（按自然日，中国时区，每天 1 次）
/// </summary>
public class UserAiOrganizeUsage
{
    public Guid UserId { get; private set; }

    /// <summary>最近一次成功使用的日期（中国时区）</summary>
    public DateOnly LastUsedDate { get; private set; }

    public DateTime LastUsedAt { get; private set; }

    private UserAiOrganizeUsage() { }

    public UserAiOrganizeUsage(Guid userId, DateOnly usedDate, DateTime usedAt)
    {
        UserId = userId;
        LastUsedDate = usedDate;
        LastUsedAt = usedAt;
    }

    public bool UsedOn(DateOnly date) => LastUsedDate == date;

    public void MarkUsed(DateOnly usedDate, DateTime usedAt)
    {
        LastUsedDate = usedDate;
        LastUsedAt = usedAt;
    }
}
