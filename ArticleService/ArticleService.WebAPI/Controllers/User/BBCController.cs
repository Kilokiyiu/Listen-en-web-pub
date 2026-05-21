using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;

namespace ArticleService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BBCController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BBCController> _logger;

    // BBC RSS Feed URLs
    private static readonly Dictionary<string, string> CategoryUrls = new()
    {
        { "world", "https://feeds.bbci.co.uk/news/world/rss.xml" },
        { "technology", "https://feeds.bbci.co.uk/news/technology/rss.xml" },
        { "business", "https://feeds.bbci.co.uk/news/business/rss.xml" },
        { "science", "https://feeds.bbci.co.uk/news/science_and_environment/rss.xml" },
        { "health", "https://feeds.bbci.co.uk/news/health/rss.xml" },
        { "entertainment", "https://feeds.bbci.co.uk/news/entertainment_and_arts/rss.xml" }
    };

    private static readonly string DefaultUrl = "https://feeds.bbci.co.uk/news/rss.xml";

    public BBCController(IHttpClientFactory httpClientFactory, ILogger<BBCController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取 BBC News 最新新闻
    /// </summary>
    /// <param name="category">可选：world, technology, business, science, health, entertainment</param>
    /// <returns>新闻列表</returns>
    [HttpGet]
    public async Task<ActionResult<List<BBCNewsItem>>> GetTopNews(string? category = null)
    {
        try
        {
            string url;
            if (!string.IsNullOrEmpty(category) && CategoryUrls.ContainsKey(category.ToLower()))
            {
                url = CategoryUrls[category.ToLower()];
            }
            else
            {
                url = DefaultUrl;
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var xmlContent = await response.Content.ReadAsStringAsync();

            var newsItems = new List<BBCNewsItem>();

            using (var stringReader = new StringReader(xmlContent))
            using (var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { Async = true }))
            {
                var feedReader = new RssFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    if (feedReader.ElementType == SyndicationElementType.Item)
                    {
                        ISyndicationItem item = await feedReader.ReadItem();

                        // 获取链接
                        string link = "";
                        if (item.Links != null)
                        {
                            var firstLink = item.Links.FirstOrDefault();
                            if (firstLink != null)
                            {
                                link = firstLink.Uri?.ToString() ?? "";
                            }
                        }

                        // 获取发布时间
                        DateTime pubDate = DateTime.Now;
                        if (item.Published != DateTimeOffset.MinValue)
                        {
                            pubDate = item.Published.DateTime;
                        }

                        newsItems.Add(new BBCNewsItem
                        {
                            Title = item.Title ?? "",
                            Description = item.Description ?? "",
                            Link = link,
                            PubDate = pubDate,
                            Category = category ?? "general"
                        });

                        if (newsItems.Count >= 30)
                            break;
                    }
                }
            }

            return Ok(new { code = 200, data = newsItems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 BBC News 失败");
            return StatusCode(500, new { code = 500, message = "获取新闻失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 获取 BBC 文章全文内容
    /// </summary>
    /// <param name="url">BBC 文章 URL</param>
    /// <returns>文章全文</returns>
    [HttpGet]
    public async Task<ActionResult<BBCArticleDetail>> GetArticleDetail(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url) || (!url.Contains("bbc.co.uk") && !url.Contains("bbc.com")))
            {
                return BadRequest(new { code = 400, message = "无效的 BBC 文章链接" });
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // 提取标题
            var title = ExtractTitle(html);

            // 提取正文内容
            var content = ExtractArticleContent(html);

            // 提取发布时间
            var pubDate = ExtractPublishDate(html);

            return Ok(new
            {
                code = 200,
                data = new BBCArticleDetail
                {
                    Title = title,
                    Content = content,
                    PubDate = pubDate,
                    OriginalUrl = url
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 BBC 文章详情失败: {Url}", url);
            return StatusCode(500, new { code = 500, message = "获取文章详情失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 获取所有可用分类
    /// </summary>
    [HttpGet]
    public ActionResult<List<CategoryInfo>> GetCategories()
    {
        var categories = CategoryUrls.Keys.Select(key => new CategoryInfo
        {
            Code = key,
            Name = key switch
            {
                "world" => "世界新闻",
                "technology" => "科技",
                "business" => "商业",
                "science" => "科学环境",
                "health" => "健康",
                "entertainment" => "娱乐艺术",
                _ => key
            }
        }).ToList();

        return Ok(new { code = 200, data = categories });
    }

    #region HTML 解析辅助方法

    private string ExtractTitle(string html)
    {
        // 尝试从 title 标签提取
        var titleMatch = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);
        if (titleMatch.Success)
        {
            var title = titleMatch.Groups[1].Value;
            // BBC 标题通常包含 " - BBC News"
            title = title.Replace(" - BBC News", "").Trim();
            return title;
        }
        return "";
    }

    private string ExtractArticleContent(string html)
    {
        // BBC 文章正文通常在 data-component="text-block" 的 div 中
        var contentBuilder = new System.Text.StringBuilder();

        // 匹配 data-component="text-block" 的段落
        var textBlockPattern = @"<div[^>]*data-component=[""]text-block[""][^>]*>\s*<p[^>]*>(.*?)</p>\s*</div>";
        var matches = Regex.Matches(html, textBlockPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                var paragraph = match.Groups[1].Value;
                // 去除 HTML 标签
                paragraph = Regex.Replace(paragraph, @"<[^>]+>", "");
                // 解码 HTML 实体
                paragraph = System.Net.WebUtility.HtmlDecode(paragraph);

                if (!string.IsNullOrWhiteSpace(paragraph))
                {
                    contentBuilder.AppendLine($"<p>{paragraph}</p>");
                }
            }
        }

        // 如果没找到 text-block，尝试其他模式
        if (contentBuilder.Length == 0)
        {
            // 尝试提取 article body
            var articlePattern = @"<article[^>]*>(.*?)</article>";
            var articleMatch = Regex.Match(html, articlePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (articleMatch.Success)
            {
                var articleContent = articleMatch.Groups[1].Value;
                // 提取所有段落
                var pMatches = Regex.Matches(articleContent, @"<p[^>]*>(.*?)</p>", RegexOptions.Singleline);
                foreach (Match pMatch in pMatches)
                {
                    var p = Regex.Replace(pMatch.Groups[1].Value, @"<[^>]+>", "");
                    p = System.Net.WebUtility.HtmlDecode(p);
                    if (!string.IsNullOrWhiteSpace(p) && p.Length > 20)
                    {
                        contentBuilder.AppendLine($"<p>{p}</p>");
                    }
                }
            }
        }

        return contentBuilder.ToString();
    }

    private DateTime ExtractPublishDate(string html)
    {
        // 尝试从 time 标签提取
        var timePattern = @"<time[^>]*datetime=[""]([^""]+)[""][^>]*>";
        var timeMatch = Regex.Match(html, timePattern, RegexOptions.IgnoreCase);
        if (timeMatch.Success)
        {
            if (DateTime.TryParse(timeMatch.Groups[1].Value, out var date))
            {
                return date;
            }
        }
        return DateTime.Now;
    }

    #endregion
}

public class BBCNewsItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Link { get; set; } = "";
    public DateTime PubDate { get; set; }
    public string Category { get; set; } = "";
}

public class BBCArticleDetail
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime PubDate { get; set; }
    public string OriginalUrl { get; set; } = "";
}

public class CategoryInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}
