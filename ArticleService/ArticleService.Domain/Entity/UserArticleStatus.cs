namespace ArticleService.Domain.Entity;

/// <summary>
/// 这个表用于记录用户与短文的关联，DailyArticle表是一个公开的表，而此表用于记录用户对于每篇文章的状态
/// 比如用户已经阅读了某篇短文，用户收藏了谋篇短文，都用此表来标记
/// </summary>
public class UserArticleStatus
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ArticleId { get; private set; }
    public bool IsRead { get; private set; }
    public bool IsFavorited { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FavoritedAt { get; private set; }

    //导航属性（可选，方便 EF Core 查询）
    public DailyArticle Article { get; private set; } = null!;

    private UserArticleStatus() { } //EFCore 需要

    public UserArticleStatus(Guid userId, Guid articleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ArticleId = articleId;
        IsRead = false;
        IsFavorited = false;
        CreatedAt = DateTime.Now;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void ToggleFavorite()
    {
        IsFavorited = !IsFavorited;
        FavoritedAt = IsFavorited ? DateTime.Now : null;
    }
}