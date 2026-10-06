using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers;

/// <summary>
/// 词根动态站点地图。对外由 nginx 映射为 /sitemap-words.xml
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class SitemapController : ControllerBase
{
    private const string SiteUrl = "https://your-domain.com";
    private const string SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly WordDbContext db;
    private readonly IMemoryCache cache;

    public SitemapController(WordDbContext db, IMemoryCache cache)
    {
        this.db = db;
        this.cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> WordRoots()
    {
        var xml = await cache.GetOrCreateAsync("sitemap:words", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var roots = await db.WordRoots
                .AsNoTracking()
                .OrderByDescending(x => x.CreationTime)
                .Select(x => new { x.Id, x.CreationTime })
                .ToListAsync();

            XNamespace ns = SitemapNs;
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset",
                    roots.Select(r => new XElement(ns + "url",
                        new XElement(ns + "loc", $"{SiteUrl}/#/word-roots/{r.Id}"),
                        new XElement(ns + "lastmod", r.CreationTime.ToUniversalTime().ToString("yyyy-MM-dd")),
                        new XElement(ns + "changefreq", "monthly"),
                        new XElement(ns + "priority", "0.5")))));
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            doc.Save(writer, SaveOptions.DisableFormatting);
            return sb.ToString();
        });

        Response.Headers.CacheControl = "public, max-age=900";
        return Content(xml!, "application/xml", Encoding.UTF8);
    }
}
