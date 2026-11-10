namespace WordService.WebAPI.Options;

public class DeepSeekOptions
{
    public const string SectionName = "DeepSeek";

    /// <summary>DeepSeek API Key（建议用环境变量 DeepSeek__ApiKey / DEEPSEEK_API_KEY）</summary>
    public string? ApiKey { get; set; }

    public string BaseUrl { get; set; } = "https://api.deepseek.com";

    public string Model { get; set; } = "deepseek-chat";

    /// <summary>单次送给模型的最大字符数（按行切块）</summary>
    public int MaxChunkChars { get; set; } = 6000;

    /// <summary>用户原文最大长度</summary>
    public int MaxInputChars { get; set; } = 200000;

    /// <summary>AI 整理合并后最多保留的词条数</summary>
    public int MaxOutputEntries { get; set; } = 5000;
}
