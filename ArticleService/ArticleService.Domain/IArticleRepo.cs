using ArticleService.Domain.Entity;

namespace ArticleService.Domain;

public interface IArticleRepo
{
    /// <summary>
    /// 获取对应日期的短文
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    Task<DailyArticle?> GetByDateAsync(DateTime date);
    
    /// <summary>
    /// 获取所有已经发布的短文
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<DailyArticle[]> GetPublishedArticlesAsync(int page, int pageSize);
    
    /// <summary>
    /// 标记为已读
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="articleId"></param>
    /// <returns></returns>
    Task MarkIsReadAsync(Guid userId, Guid articleId);
    
    /// <summary>
    /// 是否收藏短文
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="articleId"></param>
    /// <returns></returns>
    Task ToggleFavoriteAsync(Guid userId, Guid articleId);

    /// <summary>
    /// 获取用户的阅读历史
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<UserArticleStatus[]> GetReadHistoryAsync(Guid userId, int page, int pageSize);

    /// <summary>
    /// 获取用户在某篇文章上的操作状态（如不存在返回 null）
    /// </summary>
    Task<UserArticleStatus?> GetUserStatusAsync(Guid userId, Guid articleId);

    // ========== 管理员方法 ==========

    /// <summary>
    /// 获取所有文章（包括未发布的）
    /// </summary>
    Task<DailyArticle[]> GetAllArticlesAsync();

    /// <summary>
    /// 添加单篇文章
    /// </summary>
    Task<DailyArticle> AddArticleAsync(DailyArticle article);

    /// <summary>
    /// 批量添加文章
    /// </summary>
    Task<DailyArticle[]> AddArticlesAsync(IEnumerable<DailyArticle> articles);

    /// <summary>
    /// 更新文章
    /// </summary>
    Task UpdateArticleAsync(DailyArticle article);

    /// <summary>
    /// 删除文章
    /// </summary>
    Task DeleteArticleAsync(Guid articleId);

    /// <summary>
    /// 切换发布状态
    /// </summary>
    Task TogglePublishStatusAsync(Guid articleId);
}