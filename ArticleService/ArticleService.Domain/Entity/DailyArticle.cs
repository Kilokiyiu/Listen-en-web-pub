using DomainCommons;

namespace ArticleService.Domain.Entity;

public class DailyArticle : ICreationTime
{
    public Guid Id { get; private set; }
    /// <summary>
    /// 公开日期 - 到达该日期后文章自动显示
    /// </summary>
    public DateTime PublicDate { get; private set; }
    public MultilingualString Title { get; private set; }
    /// <summary>
    /// 录入日期 - 系统自动设置
    /// </summary>
    public DateTime CreationTime { get; private set; }
    public string EnglishText { get; private set; }
    public string ChineseText { get; private set; }
    public string? ArticleUrl { get; private set; }
    /// <summary>
    /// 是否已发布
    /// </summary>
    public bool IsPublished { get; private set; }

    private DailyArticle() { }

    public DailyArticle(DateTime publicDate, MultilingualString title,
        string englishText, string chineseText, string? articleUrl = null)
    {
        Id = Guid.NewGuid();
        PublicDate = publicDate.Date;
        CreationTime = DateTime.Now;
        Title = title;
        EnglishText = englishText;
        ChineseText = chineseText;
        ArticleUrl = articleUrl;
        IsPublished = false;
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;
    public void TogglePublishStatus() => IsPublished = !IsPublished;

    /// <summary>
    /// 更新文章内容
    /// </summary>
    public void Update(string englishText, string chineseText, string? articleUrl = null)
    {
        EnglishText = englishText;
        ChineseText = chineseText;
        ArticleUrl = articleUrl;
    }
}