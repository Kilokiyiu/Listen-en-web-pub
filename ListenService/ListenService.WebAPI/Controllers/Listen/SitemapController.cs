using System.Text;
using System.Xml.Linq;
using ListenService.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ListenService.WebAPI.Controllers.Listen;

/// <summary>
/// 动态站点地图。对外由 nginx 映射为 /sitemap.xml、/sitemap-pages.xml、/sitemap-albums.xml
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
[Route("/api/listen/[controller]/[action]")]
public class SitemapController : ControllerBase
{
    private const string SiteUrl = "https://your-domain.com";
    private const string SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly IListenRepo repo;
    private readonly IMemoryCache cache;

    public SitemapController(IListenRepo repo, IMemoryCache cache)
    {
        this.repo = repo;
        this.cache = cache;
    }

    /// <summary>sitemap 索引（入口）</summary>
    [HttpGet]
    public IActionResult Index()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var xml = BuildSitemapIndex(new[]
        {
            ($"{SiteUrl}/sitemap-pages.xml", today),
            ($"{SiteUrl}/sitemap-albums.xml", today),
            ($"{SiteUrl}/sitemap-articles.xml", today),
            ($"{SiteUrl}/sitemap-words.xml", today),
        });
        Response.Headers.CacheControl = "public, max-age=900";
        return Xml(xml);
    }

    /// <summary>固定栏目页</summary>
    [HttpGet]
    public IActionResult Pages()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var urls = new (string Loc, string Lastmod, string Changefreq, string Priority)[]
        {
            ($"{SiteUrl}/", today, "daily", "1.0"),
            ($"{SiteUrl}/#/exams", today, "weekly", "0.9"),
            ($"{SiteUrl}/#/daily", today, "daily", "0.8"),
            ($"{SiteUrl}/#/bbc-news", today, "daily", "0.8"),
            ($"{SiteUrl}/#/word-roots", today, "weekly", "0.7"),
            ($"{SiteUrl}/#/about", today, "monthly", "0.5"),
            ($"{SiteUrl}/#/feedback", today, "monthly", "0.3"),
        };
        Response.Headers.CacheControl = "public, max-age=900";
        return Xml(BuildUrlSet(urls));
    }

    /// <summary>可见听力真题（随后台上传自动更新）</summary>
    [HttpGet]
    public async Task<IActionResult> Albums()
    {
        var xml = await cache.GetOrCreateAsync("sitemap:albums", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var albums = await repo.GetVisibleAlbumSitemapItemsAsync();
            var urls = albums.Select(a => (
                Loc: $"{SiteUrl}/#/exam?albumId={a.Id}",
                Lastmod: a.CreationTime.ToUniversalTime().ToString("yyyy-MM-dd"),
                Changefreq: "monthly",
                Priority: "0.6"
            ));
            return BuildUrlSet(urls);
        });
        Response.Headers.CacheControl = "public, max-age=900";
        return Xml(xml!);
    }

    private static string BuildSitemapIndex(IEnumerable<(string Loc, string Lastmod)> items)
    {
        XNamespace ns = SitemapNs;
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "sitemapindex",
                items.Select(i => new XElement(ns + "sitemap",
                    new XElement(ns + "loc", i.Loc),
                    new XElement(ns + "lastmod", i.Lastmod)))));
        return ToXmlString(doc);
    }

    private static string BuildUrlSet(IEnumerable<(string Loc, string Lastmod, string Changefreq, string Priority)> urls)
    {
        XNamespace ns = SitemapNs;
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset",
                urls.Select(u => new XElement(ns + "url",
                    new XElement(ns + "loc", u.Loc),
                    new XElement(ns + "lastmod", u.Lastmod),
                    new XElement(ns + "changefreq", u.Changefreq),
                    new XElement(ns + "priority", u.Priority)))));
        return ToXmlString(doc);
    }

    private static string ToXmlString(XDocument doc)
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        doc.Save(writer, SaveOptions.DisableFormatting);
        return sb.ToString();
    }

    private ContentResult Xml(string xml) => Content(xml, "application/xml", Encoding.UTF8);
}
