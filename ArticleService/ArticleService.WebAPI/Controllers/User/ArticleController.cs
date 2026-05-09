using System.Security.Claims;
using ArticleService.Domain;
using ArticleService.Domain.Entity;
using ArticleService.Infrastructure;
using ArticleService.WebAPI.Controllers.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]

public class ArticleController : ControllerBase
{
    private readonly IArticleRepo repo;

    public ArticleController(IArticleRepo repo)
    {
        this.repo = repo;
    }

    /// <summary>
    /// 获取对应日期的文章
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<DailyArticle>> GetArticleByDate(DateTime date)
    {
        var article = await repo.GetByDateAsync(date);
        if (article == null)
        {
            return BadRequest("文章不存在");
        }

        // 查询当前用户对该文章的已读/收藏状态
        var userId = await GetCurrentUserId();
        var userStatus = await repo.GetUserStatusAsync(userId, article.Id);

        var dto = new DailyArticleRespons
        {
            Id = article.Id,
            PublicDate = article.PublicDate,
            TitleChinese = article.Title.Chinese,
            TitleEnglish = article.Title.English,
            EnglishText = article.EnglishText,
            ChineseText = article.ChineseText,
            AudioUrl = article.ArticleUrl,
            IsRead = userStatus?.IsRead ?? false,
            IsFavorite = userStatus?.IsFavorited ?? false
        };
        return Ok(dto);
    }

    /// <summary>
    /// 将文章标记为已读
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> MarkIsRead([FromBody] ArticleRequest request)
    {
        var userId = await GetCurrentUserId();
        await repo.MarkIsReadAsync(userId, request.ArticleId);
        return Ok();
    }

    /// <summary>
    /// 收藏对应文章
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> ToggleFavoriteAsync([FromBody] ArticleRequest request)
    {
        var userId = await GetCurrentUserId();
        await repo.ToggleFavoriteAsync(userId, request.ArticleId);
        return Ok();
    }


    /// <summary>
    /// 获取用户的阅读记录
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<ReadHistoryResponse[]>> GetReadHistoryAsync(int page = 1, int pageSize = 20)
    {
        var userId = await GetCurrentUserId();
        var history = await repo.GetReadHistoryAsync(userId, page, pageSize);

        var dto = history.Select(e => new ReadHistoryResponse
        {
            ArticleId = e.Article.Id,
            TitleChinese = e.Article.Title.Chinese,
            TitleEnglish = e.Article.Title.English,
            PublicDate = e.Article.PublicDate,
            IsFavorited = e.IsFavorited,
            CreatedAt = e.CreatedAt
        }).ToArray();
        
        return Ok(dto);
    }


    private async Task<Guid> GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim))
        {
            throw new UnauthorizedAccessException("用户Id不存在");
        }
        return Guid.Parse(claim);
    }
}