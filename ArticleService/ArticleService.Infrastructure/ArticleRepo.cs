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

    public Task<DailyArticle?> GetByDateAsync(DateTime date)
    {
        return dbContext.DailyArticles.FirstOrDefaultAsync(e => e.PublicDate == date.Date && e.IsPublished);
    }

    public Task<DailyArticle[]> GetPublishedArticlesAsync(int page, int pageSize)
    {
        return dbContext.DailyArticles
            .Where(e => e.IsPublished)
            .OrderByDescending(e => e.PublicDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
    }

    public async Task<(DateTime PublicDate, DateTime CreationTime)[]> GetPublishedSitemapItemsAsync()
    {
        var items = await dbContext.DailyArticles
            .AsNoTracking()
            .Where(e => e.IsPublished)
            .OrderByDescending(e => e.PublicDate)
            .Select(e => new { e.PublicDate, e.CreationTime })
            .ToListAsync();
        return items.Select(e => (e.PublicDate, e.CreationTime)).ToArray();
    }

    public async Task MarkIsReadAsync(Guid userId, Guid articleId)
    {
        var status = await dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ArticleId == articleId);

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

    public Task<UserArticleStatus?> GetUserStatusAsync(Guid userId, Guid articleId)
    {
        return dbContext.UserArticleStatuses
            .FirstOrDefaultAsync(e => e.UserId == userId && e.ArticleId == articleId);
    }

    // ========== 管理员方法 ==========

    public Task<DailyArticle[]> GetAllArticlesAsync()
    {
        return dbContext.DailyArticles
            .OrderByDescending(e => e.PublicDate)
            .ToArrayAsync();
    }

    public async Task<DailyArticle> AddArticleAsync(DailyArticle article)
    {
        dbContext.DailyArticles.Add(article);
        await dbContext.SaveChangesAsync();
        return article;
    }

    public async Task<DailyArticle[]> AddArticlesAsync(IEnumerable<DailyArticle> articles)
    {
        var articleList = articles.ToList();
        dbContext.DailyArticles.AddRange(articleList);
        await dbContext.SaveChangesAsync();
        return articleList.ToArray();
    }

    public async Task UpdateArticleAsync(DailyArticle article)
    {
        dbContext.DailyArticles.Update(article);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteArticleAsync(Guid articleId)
    {
        var article = await dbContext.DailyArticles.FindAsync(articleId);
        if (article != null)
        {
            dbContext.DailyArticles.Remove(article);
            await dbContext.SaveChangesAsync();
        }
    }

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
