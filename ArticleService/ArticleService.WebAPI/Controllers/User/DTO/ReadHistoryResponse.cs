namespace ArticleService.WebAPI.Controllers.DTO;

public class ReadHistoryResponse
{
    public Guid ArticleId { get; set; }
    public string TitleChinese { get; set; } = "";
    public string TitleEnglish { get; set; } = "";
    /// <summary>
    /// 公开日期
    /// </summary>
    public DateTime PublicDate { get; set; }
    public bool IsFavorited { get; set; }
    public DateTime CreatedAt { get; set; }
}
