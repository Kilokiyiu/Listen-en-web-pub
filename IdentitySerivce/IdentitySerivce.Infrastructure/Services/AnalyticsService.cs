using IdentitySerivce.Domain.Entity;
using IdentitySerivce.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MyCache;

namespace IdentitySerivce.Infrastructure.Services;

public interface IAnalyticsService
{
    Task TrackPageViewAsync(string path, string visitorId, Guid? userId, CancellationToken cancellationToken = default);
    Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyCountDto>> GetRegistrationTrendAsync(int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyTrafficDto>> GetTrafficTrendAsync(int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopPageDto>> GetTopPagesAsync(int days, int limit, CancellationToken cancellationToken = default);
    Task AggregateYesterdayAsync(CancellationToken cancellationToken = default);
    Task PurgeOldEventsAsync(int retentionDays, CancellationToken cancellationToken = default);
}

public record AnalyticsOverviewDto(
    int TotalUsers,
    int NewUsersToday,
    int NewUsersLast7Days,
    int NewUsersLast30Days,
    int TodayPageViews,
    int TodayUniqueVisitors);

public record DailyCountDto(string Date, int Count);

public record DailyTrafficDto(string Date, int PageViews, int UniqueVisitors);

public record TopPageDto(string Path, int PageViews, int UniqueVisitors);

public class AnalyticsService : IAnalyticsService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IdentityDbContext db;
    private readonly ICacheService cache;

    public AnalyticsService(IdentityDbContext db, ICacheService cache)
    {
        this.db = db;
        this.cache = cache;
    }

    public async Task TrackPageViewAsync(string path, string visitorId, Guid? userId, CancellationToken cancellationToken = default)
    {
        db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            EventType = "page_view",
            Path = path,
            VisitorId = visitorId,
            UserId = userId,
            CreatedAt = DateTime.Now,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default) =>
        cache.GetOrSetAsync("analytics:overview", async () =>
        {
            var now = DateTime.Now;
            var todayStart = now.Date;
            var day7 = todayStart.AddDays(-7);
            var day30 = todayStart.AddDays(-30);

            var regularUserIds = await GetRegularUserIdsAsync(cancellationToken);
            var usersQuery = db.Users.Where(u => regularUserIds.Contains(u.Id));

            var totalUsers = await usersQuery.CountAsync(cancellationToken);
            var newToday = await usersQuery.CountAsync(u => u.CreationTime >= todayStart, cancellationToken);
            var new7 = await usersQuery.CountAsync(u => u.CreationTime >= day7, cancellationToken);
            var new30 = await usersQuery.CountAsync(u => u.CreationTime >= day30, cancellationToken);

            var todayPv = await db.AnalyticsEvents
                .CountAsync(e => e.EventType == "page_view" && e.CreatedAt >= todayStart, cancellationToken);
            var todayUv = await db.AnalyticsEvents
                .Where(e => e.EventType == "page_view" && e.CreatedAt >= todayStart)
                .Select(e => e.VisitorId)
                .Distinct()
                .CountAsync(cancellationToken);

            return new AnalyticsOverviewDto(totalUsers, newToday, new7, new30, todayPv, todayUv);
        }, CacheDuration);

    public Task<IReadOnlyList<DailyCountDto>> GetRegistrationTrendAsync(int days, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, 90);
        return cache.GetOrSetAsync($"analytics:registrations:{days}", async () =>
        {
            var regularUserIds = await GetRegularUserIdsAsync(cancellationToken);
            var start = DateTime.Now.Date.AddDays(-(days - 1));

            var grouped = await db.Users
                .Where(u => regularUserIds.Contains(u.Id) && u.CreationTime >= start)
                .GroupBy(u => u.CreationTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var map = grouped.ToDictionary(x => DateOnly.FromDateTime(x.Date), x => x.Count);
            var result = new List<DailyCountDto>();
            for (var i = 0; i < days; i++)
            {
                var date = DateOnly.FromDateTime(start.AddDays(i));
                map.TryGetValue(date, out var count);
                result.Add(new DailyCountDto(date.ToString("yyyy-MM-dd"), count));
            }

            return (IReadOnlyList<DailyCountDto>)result;
        }, CacheDuration);
    }

    public Task<IReadOnlyList<DailyTrafficDto>> GetTrafficTrendAsync(int days, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, 90);
        return cache.GetOrSetAsync($"analytics:traffic:{days}", async () =>
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-(days - 1)));
            var endDate = DateOnly.FromDateTime(DateTime.Now.Date);

            var dailyRows = await db.AnalyticsDailies
                .Where(d => d.Date >= startDate && d.Date < endDate)
                .GroupBy(d => d.Date)
                .Select(g => new DailyTrafficDto(
                    g.Key.ToString("yyyy-MM-dd"),
                    g.Sum(x => x.PageViews),
                    g.Sum(x => x.UniqueVisitors)))
                .ToListAsync(cancellationToken);

            var dailyMap = dailyRows.ToDictionary(x => x.Date, x => x);

            var todayStart = DateTime.Now.Date;
            var todayPv = await db.AnalyticsEvents
                .CountAsync(e => e.EventType == "page_view" && e.CreatedAt >= todayStart, cancellationToken);
            var todayUv = await db.AnalyticsEvents
                .Where(e => e.EventType == "page_view" && e.CreatedAt >= todayStart)
                .Select(e => e.VisitorId)
                .Distinct()
                .CountAsync(cancellationToken);

            var result = new List<DailyTrafficDto>();
            for (var i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var key = date.ToString("yyyy-MM-dd");
                if (date == endDate)
                {
                    result.Add(new DailyTrafficDto(key, todayPv, todayUv));
                }
                else if (dailyMap.TryGetValue(key, out var row))
                {
                    result.Add(row);
                }
                else
                {
                    result.Add(new DailyTrafficDto(key, 0, 0));
                }
            }

            return (IReadOnlyList<DailyTrafficDto>)result;
        }, CacheDuration);
    }

    public Task<IReadOnlyList<TopPageDto>> GetTopPagesAsync(int days, int limit, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, 90);
        limit = Math.Clamp(limit, 1, 50);
        return cache.GetOrSetAsync($"analytics:top-pages:{days}:{limit}", async () =>
        {
            var start = DateTime.Now.Date.AddDays(-days + 1);
            var startDate = DateOnly.FromDateTime(start);

            var fromDaily = await db.AnalyticsDailies
                .Where(d => d.Date >= startDate && d.Date < DateOnly.FromDateTime(DateTime.Now.Date))
                .GroupBy(d => d.Path)
                .Select(g => new
                {
                    Path = g.Key,
                    PageViews = g.Sum(x => x.PageViews),
                    UniqueVisitors = g.Sum(x => x.UniqueVisitors),
                })
                .ToListAsync(cancellationToken);

            var fromToday = await db.AnalyticsEvents
                .Where(e => e.EventType == "page_view" && e.CreatedAt >= DateTime.Now.Date)
                .GroupBy(e => e.Path)
                .Select(g => new
                {
                    Path = g.Key,
                    PageViews = g.Count(),
                    UniqueVisitors = g.Select(x => x.VisitorId).Distinct().Count(),
                })
                .ToListAsync(cancellationToken);

            var merged = fromDaily
                .Concat(fromToday)
                .GroupBy(x => x.Path)
                .Select(g => new TopPageDto(
                    g.Key,
                    g.Sum(x => x.PageViews),
                    g.Sum(x => x.UniqueVisitors)))
                .OrderByDescending(x => x.PageViews)
                .Take(limit)
                .ToList();

            return (IReadOnlyList<TopPageDto>)merged;
        }, CacheDuration);
    }

    public async Task AggregateYesterdayAsync(CancellationToken cancellationToken = default)
    {
        var yesterday = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-1));
        var start = yesterday.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var exists = await db.AnalyticsDailies.AnyAsync(d => d.Date == yesterday, cancellationToken);
        if (exists)
        {
            return;
        }

        var groups = await db.AnalyticsEvents
            .Where(e => e.EventType == "page_view" && e.CreatedAt >= start && e.CreatedAt < end)
            .GroupBy(e => e.Path)
            .Select(g => new
            {
                Path = g.Key,
                PageViews = g.Count(),
                UniqueVisitors = g.Select(x => x.VisitorId).Distinct().Count(),
            })
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            db.AnalyticsDailies.Add(new AnalyticsDaily
            {
                Date = yesterday,
                Path = group.Path,
                PageViews = group.PageViews,
                UniqueVisitors = group.UniqueVisitors,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task PurgeOldEventsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.Now.Date.AddDays(-retentionDays);
        await db.AnalyticsEvents
            .Where(e => e.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<List<Guid>> GetRegularUserIdsAsync(CancellationToken cancellationToken)
    {
        var adminUserIds = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where r.Name == "Admin"
            select ur.UserId).ToListAsync(cancellationToken);

        return await db.Users
            .Where(u => !adminUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
}
