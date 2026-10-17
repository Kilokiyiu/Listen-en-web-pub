using ListenService.Domain.Entity;
using ListenService.Infrastrucure;
using Microsoft.EntityFrameworkCore;

namespace ListenService.WebAPI;

public static class AnnouncementSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ListenDbContext>();

        if (!await db.SiteRuntimeInfos.AnyAsync(x => x.Id == SiteRuntimeInfo.DefaultId))
        {
            db.SiteRuntimeInfos.Add(SiteRuntimeInfo.CreateDefault());
            logger.LogInformation("Seeded default site runtime info.");
        }

        if (!await db.Announcements.AnyAsync())
        {
            db.Announcements.Add(new Announcement(
                "考研英语上线",
                "考研英语内容已正式上线，可练习英语一 / 英语二历年完形填空，提交后查看答案与解析。",
                new DateTime(2026, 10, 14),
                isPinned: true));
            logger.LogInformation("Seeded initial platform announcement.");
        }

        await db.SaveChangesAsync();
    }
}
