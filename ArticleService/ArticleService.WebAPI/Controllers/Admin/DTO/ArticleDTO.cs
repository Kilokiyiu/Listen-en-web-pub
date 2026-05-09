namespace ArticleService.WebAPI.Controllers.Admin.DTO;

public class ArticleDto
{
    public Guid? Id { get; set; }
    /// <summary>
    /// 公开日期 - 到达该日期后文章自动显示
    /// </summary>
    public DateTime PublicDate { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string ChineseText { get; set; } = "";
    public string? ArticleUrl { get; set; }
    /// <summary>
    /// 是否已发布
    /// </summary>
    public bool IsPublished { get; set; }
    /// <summary>
    /// 录入日期 - 系统自动设置
    /// </summary>
    public DateTime CreationTime { get; set; }
}

public class AddArticleRequest
{
    /// <summary>
    /// 公开日期 - 到达该日期后文章自动显示
    /// </summary>
    public DateTime PublicDate { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string ChineseText { get; set; } = "";
    public string? ArticleUrl { get; set; }
}

public class BatchAddArticlesRequest
{
    public List<AddArticleRequest> Articles { get; set; } = new();
}

public class UpdateArticleRequest
{
    public Guid Id { get; set; }
    public string EnglishText { get; set; } = "";
    public string ChineseText { get; set; } = "";
    public string? ArticleUrl { get; set; }
}
