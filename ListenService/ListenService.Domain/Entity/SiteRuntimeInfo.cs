using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 站点运行状态（单例行）
/// </summary>
public class SiteRuntimeInfo : IEntity
{
    public static readonly Guid DefaultId = Guid.Parse("a1111111-1111-4111-8111-111111111111");

    /// <summary>Normal / Notice / Maintenance</summary>
    public const string StatusNormal = "Normal";
    public const string StatusNotice = "Notice";
    public const string StatusMaintenance = "Maintenance";

    public Guid Id { get; private set; }
    public string Status { get; private set; } = StatusNormal;
    public string StatusText { get; private set; } = "网站运行正常";
    public string? ExtraInfo { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SiteRuntimeInfo()
    {
    }

    public static SiteRuntimeInfo CreateDefault()
    {
        return new SiteRuntimeInfo
        {
            Id = DefaultId,
            Status = StatusNormal,
            StatusText = "网站运行正常",
            ExtraInfo = "听力 / 阅读 / 考研 / 词根等服务可用",
            UpdatedAt = DateTime.Now
        };
    }

    public SiteRuntimeInfo Update(string status, string statusText, string? extraInfo)
    {
        Status = string.IsNullOrWhiteSpace(status) ? StatusNormal : status.Trim();
        StatusText = string.IsNullOrWhiteSpace(statusText) ? "网站运行正常" : statusText.Trim();
        ExtraInfo = string.IsNullOrWhiteSpace(extraInfo) ? null : extraInfo.Trim();
        UpdatedAt = DateTime.Now;
        return this;
    }
}
