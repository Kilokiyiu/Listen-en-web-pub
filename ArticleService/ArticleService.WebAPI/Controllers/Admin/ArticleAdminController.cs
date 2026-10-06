using ArticleService.Domain;
using ArticleService.Domain.Entity;
using ArticleService.Infrastructure;
using ArticleService.WebAPI.Controllers.Admin.DTO;
using DomainCommons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.WebAPI.Controllers.Admin;

[ApiController]
[Route("Admin/[action]")]
[Authorize]
public class ArticleAdminController : ControllerBase
{
    private readonly IArticleRepo repo;
    private readonly ArticleDbContext dbContext;

    public ArticleAdminController(IArticleRepo repo, ArticleDbContext dbContext)
    {
        this.repo = repo;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 阅读统计（管理后台）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetReadingStats()
    {
        var last7Days = DateTime.Now.Date.AddDays(-7);
        var totalReads = await dbContext.UserArticleStatuses.CountAsync(x => x.IsRead);
        var readsLast7Days = await dbContext.UserArticleStatuses.CountAsync(x => x.IsRead && x.CreatedAt >= last7Days);
        var totalFavorites = await dbContext.UserArticleStatuses.CountAsync(x => x.IsFavorited);
        return Ok(new
        {
            code = 200,
            data = new
            {
                totalReads,
                readsLast7Days,
                totalFavorites,
            },
        });
    }

    /// <summary>
    /// 获取所有文章（包括未发布的）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ArticleDto[]>> GetAllArticles()
    {
        var articles = await repo.GetAllArticlesAsync();
        return Ok(articles.Select(ToDto).ToArray());
    }

    /// <summary>
    /// 添加单篇文章
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArticleDto>> AddArticle([FromBody] AddArticleRequest request)
    {
        var title = new MultilingualString(request.TitleChinese, request.TitleEnglish);
        var article = new DailyArticle(
            request.PublicDate,
            title,
            request.EnglishText,
            request.ChineseText,
            request.ArticleUrl
        );
        article.Publish();
        var result = await repo.AddArticleAsync(article);
        return Ok(ToDto(result));
    }

    /// <summary>
    /// 批量添加文章
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArticleDto[]>> BatchAddArticles([FromBody] BatchAddArticlesRequest request)
    {
        if (request.Articles == null || request.Articles.Count == 0)
        {
            return BadRequest("文章列表不能为空");
        }

        var articles = request.Articles.Select(req =>
        {
            var title = new MultilingualString(req.TitleChinese, req.TitleEnglish);
            var article = new DailyArticle(
                req.PublicDate,
                title,
                req.EnglishText,
                req.ChineseText,
                req.ArticleUrl
            );
            article.Publish();
            return article;
        }).ToList();

        var result = await repo.AddArticlesAsync(articles);
        return Ok(result.Select(ToDto).ToArray());
    }

    /// <summary>
    /// 更新文章公开日期
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> UpdatePublicDate([FromBody] UpdatePublicDateRequest request)
    {
        var article = await dbContext.DailyArticles.FindAsync(request.Id);
        if (article == null)
        {
            return NotFound("文章不存在");
        }

        var newDate = request.PublicDate.Date;
        if (article.PublicDate.Date != newDate)
        {
            var conflict = await dbContext.DailyArticles
                .AnyAsync(a => a.Id != request.Id && a.PublicDate == newDate);
            if (conflict)
            {
                return BadRequest("该公开日期已有其他文章，请选择其他日期");
            }

            article.SetPublicDate(newDate);
            await dbContext.SaveChangesAsync();
        }

        return Ok(new { message = "公开日期已更新", data = ToDto(article) });
    }

    /// <summary>
    /// 更新文章内容
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> UpdateArticle([FromBody] UpdateArticleRequest request)
    {
        var article = await dbContext.DailyArticles.FindAsync(request.Id);
        if (article == null)
        {
            return NotFound("文章不存在");
        }

        article.Update(request.EnglishText, request.ChineseText, request.ArticleUrl);
        await repo.UpdateArticleAsync(article);
        return Ok(new { message = "文章已更新", data = ToDto(article) });
    }

    /// <summary>
    /// 删除文章
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> DeleteArticle([FromBody] GuidRequest request)
    {
        await repo.DeleteArticleAsync(request.Id);
        return Ok(new { message = "删除成功" });
    }

    /// <summary>
    /// 切换发布状态
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> TogglePublishStatus([FromBody] GuidRequest request)
    {
        await repo.TogglePublishStatusAsync(request.Id);
        return Ok(new { message = "状态已切换" });
    }

    private static ArticleDto ToDto(DailyArticle article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            PublicDate = article.PublicDate,
            TitleChinese = article.Title.Chinese,
            TitleEnglish = article.Title.English,
            EnglishText = article.EnglishText,
            ChineseText = article.ChineseText,
            ArticleUrl = article.ArticleUrl,
            IsPublished = article.IsPublished,
            CreationTime = article.CreationTime
        };
    }
}

public class GuidRequest
{
    public Guid Id { get; set; }
}
