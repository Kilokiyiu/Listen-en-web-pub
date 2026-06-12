namespace WordService.WebAPI.Options;

public class XxApiOptions
{
    public const string SectionName = "XxApi";

    public string EnglishWordsUrl { get; set; } = "https://v2.xxapi.cn/api/englishwords";

    /// <summary>可选，配置后优先使用 xxapi 搜狗翻译；未配置时使用免费翻译接口。</summary>
    public string? ApiKey { get; set; }

    public string SgTranslateUrl { get; set; } = "https://v2.xxapi.cn/api/sgtranslate";

    public string FallbackTranslateUrl { get; set; } = "https://translate.appworlds.cn";
}
