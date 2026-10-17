using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 平台公告
/// </summary>
public class Announcement : IEntity, ICreationTime
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime PublishDate { get; private set; }
    public bool IsPublished { get; private set; }
    public bool IsPinned { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreationTime { get; init; }
    public DateTime UpdatedAt { get; private set; }

    private Announcement()
    {
    }

    public Announcement(string title, string content, DateTime publishDate, bool isPinned = false, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        Title = title.Trim();
        Content = content.Trim();
        PublishDate = publishDate.Date;
        IsPublished = true;
        IsPinned = isPinned;
        SortOrder = sortOrder;
        CreationTime = DateTime.Now;
        UpdatedAt = CreationTime;
    }

    public Announcement Update(string title, string content, DateTime publishDate, bool isPinned, int sortOrder)
    {
        Title = title.Trim();
        Content = content.Trim();
        PublishDate = publishDate.Date;
        IsPinned = isPinned;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.Now;
        return this;
    }

    public Announcement Publish()
    {
        IsPublished = true;
        UpdatedAt = DateTime.Now;
        return this;
    }

    public Announcement Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTime.Now;
        return this;
    }
}
