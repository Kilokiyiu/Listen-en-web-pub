namespace ArticleService.WebAPI.Controllers.DTO;

public class DailyArticleRespons
{
    public Guid Id { get; set; }
    /// <summary>
    /// 公开日期 - 到达该日期后文章自动显示
    /// </summary>
    public DateTime PublicDate { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string ChineseText { get; set; } = "";
    public string? AudioUrl { get; set; }
    public bool IsRead { get; set; }       // 当前用户是否已读
    public bool IsFavorite { get; set; }    // 当前用户是否收藏
}
