using System.Text;
using System.Xml.Linq;
using ArticleService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ArticleService.WebAPI.Controllers;

/// <summary>
/// 每日短文动态站点地图。对外由 nginx 映射为 /sitemap-articles.xml
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("[controller]/[action]")]
public class SitemapController : ControllerBase
{
    private const string SiteUrl = "https://your-domain.com";
    private const string SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly IArticleRepo repo;
    private readonly IMemoryCache cache;

    public SitemapController(IArticleRepo repo, IMemoryCache cache)
    {
        this.repo = repo;
        this.cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Articles()
    {
        var xml = await cache.GetOrCreateAsync("sitemap:articles", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var articles = await repo.GetPublishedSitemapItemsAsync();
            XNamespace ns = SitemapNs;
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset",
                    articles.Select(a =>
                    {
                        var lastmod = (a.CreationTime > a.PublicDate ? a.CreationTime : a.PublicDate)
                            .ToUniversalTime()
                            .ToString("yyyy-MM-dd");
                        var date = a.PublicDate.ToString("yyyy-MM-dd");
                        return new XElement(ns + "url",
                            new XElement(ns + "loc", $"{SiteUrl}/#/daily?date={date}"),
                            new XElement(ns + "lastmod", lastmod),
                            new XElement(ns + "changefreq", "monthly"),
                            new XElement(ns + "priority", "0.6"));
                    })));
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            doc.Save(writer, SaveOptions.DisableFormatting);
            return sb.ToString();
        });

        Response.Headers.CacheControl = "public, max-age=900";
        return Content(xml!, "application/xml", Encoding.UTF8);
    }
}
