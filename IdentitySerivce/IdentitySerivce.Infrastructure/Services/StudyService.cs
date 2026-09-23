using IdentitySerivce.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace IdentitySerivce.Infrastructure.Services;

public interface IStudyService
{
    Task<StudyActivityDto> RecordAsync(Guid userId, RecordStudyRequest request, CancellationToken cancellationToken = default);
    Task<StudySummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<StudyActivityDto> Items, int Total)> GetListAsync(
        Guid userId, string? activityType, string? category, int page, int pageSize, CancellationToken cancellationToken = default);
}

public record RecordStudyRequest(
    string ActivityType,
    string ContentId,
    string Title,
    string Category,
    int DurationSeconds);

public record StudyActivityDto(
    long Id,
    string ActivityType,
    string ContentId,
    string Title,
    string Category,
    int DurationSeconds,
    DateTime UpdatedAt);

public record StudySummaryDto(
    int TotalListen,
    int TotalArticle,
    int TotalMinutes,
    int StreakDays,
    int SessionsLast7Days);

public class StudyService : IStudyService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "listen", "article",
    };

    private readonly IdentityDbContext db;

    public StudyService(IdentityDbContext db)
    {
        this.db = db;
    }

    public async Task<StudyActivityDto> RecordAsync(Guid userId, RecordStudyRequest request, CancellationToken cancellationToken = default)
    {
        var activityType = (request.ActivityType ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(activityType))
        {
            throw new ArgumentException("invalid activity type");
        }

        var contentId = (request.ContentId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(contentId) || contentId.Length > 64)
        {
            throw new ArgumentException("invalid content id");
        }

        var title = string.IsNullOrWhiteSpace(request.Title) ? "未命名内容" : request.Title.Trim();
        if (title.Length > 200) title = title[..200];

        var category = string.IsNullOrWhiteSpace(request.Category) ? "other" : request.Category.Trim();
        if (category.Length > 32) category = category[..32];

        var duration = Math.Clamp(request.DurationSeconds, 0, 8 * 3600);
        var now = DateTime.Now;

        var existing = await db.StudyActivities
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.ActivityType == activityType && x.ContentId == contentId,
                cancellationToken);

        if (existing == null)
        {
            existing = new StudyActivity
            {
                UserId = userId,
                ActivityType = activityType,
                ContentId = contentId,
                Title = title,
                Category = category,
                DurationSeconds = duration,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.StudyActivities.Add(existing);
        }
        else
        {
            existing.Title = title;
            existing.Category = category;
            // 同内容再次完成：累加时长，刷新完成时间
            existing.DurationSeconds = Math.Clamp(existing.DurationSeconds + duration, 0, 8 * 3600);
            existing.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(existing);
    }

    public async Task<StudySummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await db.StudyActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new { x.ActivityType, x.DurationSeconds, x.UpdatedAt })
            .ToListAsync(cancellationToken);

        var totalListen = items.Count(x => x.ActivityType == "listen");
        var totalArticle = items.Count(x => x.ActivityType == "article");
        var totalMinutes = (int)Math.Round(items.Sum(x => x.DurationSeconds) / 60.0);
        var day7 = DateTime.Now.Date.AddDays(-6);
        var sessionsLast7 = items.Count(x => x.UpdatedAt >= day7);

        return new StudySummaryDto(totalListen, totalArticle, totalMinutes, CalcStreak(items.Select(x => x.UpdatedAt)), sessionsLast7);
    }

    public async Task<(IReadOnlyList<StudyActivityDto> Items, int Total)> GetListAsync(
        Guid userId, string? activityType, string? category, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.StudyActivities.AsNoTracking().Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(activityType) && AllowedTypes.Contains(activityType.Trim()))
        {
            var type = activityType.Trim().ToLowerInvariant();
            query = query.Where(x => x.ActivityType == type);
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "all")
        {
            var cat = category.Trim();
            query = query.Where(x => x.Category == cat);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.Select(ToDto).ToList(), total);
    }

    private static StudyActivityDto ToDto(StudyActivity x) =>
        new(x.Id, x.ActivityType, x.ContentId, x.Title, x.Category, x.DurationSeconds, x.UpdatedAt);

    private static int CalcStreak(IEnumerable<DateTime> timestamps)
    {
        var days = timestamps
            .Select(t => DateOnly.FromDateTime(t))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (days.Count == 0) return 0;

        var today = DateOnly.FromDateTime(DateTime.Now);
        // 允许昨天起步（今天还没学不算断）
        var cursor = days[0] >= today.AddDays(-1) ? days[0] : DateOnly.MinValue;
        if (cursor == DateOnly.MinValue) return 0;

        var streak = 0;
        foreach (var day in days)
        {
            if (day == cursor)
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            else if (day < cursor)
            {
                break;
            }
        }

        return streak;
    }
}
