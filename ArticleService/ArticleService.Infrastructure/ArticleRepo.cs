using ArticleService.Domain;
using ArticleService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure;

public class ArticleRepo : IArticleRepo
{
    private readonly ArticleDbContext dbContext;

    public ArticleRepo(ArticleDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    /// <summary>
    /// 获取日期对应的文章
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public Task<DailyArticle?> GetByDateAsync(DateTime date)
    {
        return dbContext.DailyArticles.FirstOrDefaultAsync(e => e.PublicDate == date.Date && e.IsPublished);
    }

    /// <summary>
    /// 获取已经发布的文章
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public Task<DailyArticle[]> GetPublishedArticlesAsync(int page, int pageSize)
    {
        return dbContext.DailyArticles
            .Where(e => e.IsPublished)
            .OrderByDescending(e => e.PublicDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
    }

    /// <summary>
    /// 建立用户与文章之间的操作记录
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="articleId"></param>
    public async Task MarkIsReadAsync(Guid userId, Guid articleId)
    {
        //查找用户是否操作过当前文章
        var status = await dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ArticleId == articleId);

        //如果用户没有操作过当前文章或者没有阅读过，则新建一个操作记录
        if (status == null)
        {
            status = new UserArticleStatus(userId, articleId);
            status.MarkAsRead();
            dbContext.UserArticleStatuses.Add(status);
        }
        else if (!status.IsRead)
        {
            status.MarkAsRead();
        }
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 更新用户对于文章的收藏，默认为false，每触发一次取反
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="articleId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ToggleFavoriteAsync(Guid userId, Guid articleId)
    {
        var status = await dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ArticleId == articleId);
        if (status == null)
        {
            status = new UserArticleStatus(userId, articleId);
            status.ToggleFavorite();
            dbContext.UserArticleStatuses.Add(status);
        }
        else
        {
            status.ToggleFavorite();
        }
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 获取用户的阅读记录
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<UserArticleStatus[]> GetReadHistoryAsync(Guid userId, int page, int pageSize)
    {
        return dbContext.UserArticleStatuses
            .Where(e => e.UserId == userId && e.IsRead)
            .Include(e => e.Article)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
    }

    /// <summary>
    /// 获取用户在某篇文章上的操作状态
    /// </summary>
    public Task<UserArticleStatus?> GetUserStatusAsync(Guid userId, Guid articleId)
    {
        return dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ArticleId == articleId);
    }

    // ========== 管理员方法 ==========

    /// <summary>
    /// 获取所有文章（包括未发布的）
    /// </summary>
    public Task<DailyArticle[]> GetAllArticlesAsync()
    {
        return dbContext.DailyArticles
            .OrderByDescending(e => e.PublicDate)
            .ToArrayAsync();
    }

    /// <summary>
    /// 添加单篇文章
    /// </summary>
    public async Task<DailyArticle> AddArticleAsync(DailyArticle article)
    {
        dbContext.DailyArticles.Add(article);
        await dbContext.SaveChangesAsync();
        return article;
    }

    /// <summary>
    /// 批量添加文章
    /// </summary>
    public async Task<DailyArticle[]> AddArticlesAsync(IEnumerable<DailyArticle> articles)
    {
        var articleList = articles.ToList();
        dbContext.DailyArticles.AddRange(articleList);
        await dbContext.SaveChangesAsync();
        return articleList.ToArray();
    }

    /// <summary>
    /// 更新文章
    /// </summary>
    public async Task UpdateArticleAsync(DailyArticle article)
    {
        dbContext.DailyArticles.Update(article);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 删除文章
    /// </summary>
    public async Task DeleteArticleAsync(Guid articleId)
    {
        var article = await dbContext.DailyArticles.FindAsync(articleId);
        if (article != null)
        {
            dbContext.DailyArticles.Remove(article);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 切换发布状态
    /// </summary>
    public async Task TogglePublishStatusAsync(Guid articleId)
    {
        var article = await dbContext.DailyArticles.FindAsync(articleId);
        if (article != null)
        {
            article.TogglePublishStatus();
            await dbContext.SaveChangesAsync();
        }
    }
}