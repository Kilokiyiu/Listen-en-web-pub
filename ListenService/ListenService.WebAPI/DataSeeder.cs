using DomainCommons;
using ListenService.Domain.Entity;
using ListenService.Infrastrucure;
using Microsoft.EntityFrameworkCore;

namespace ListenService.WebAPI;

public static class DataSeeder
{
    public static async Task SeedAsync(ListenDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
        {
            return; 
        }
        
        //测试用
        var cet6 = new Category(
            new MultilingualString("大学英语六级", "CET-6"),
            "cet6",
            1,
            "/images/cet6.png"
        );

        dbContext.Categories.Add(cet6);
        
        var album2016June1 = new Album(
            new MultilingualString("2016年6月大学英语六级听力真题（第1套）", "CET-6 June 2016 (Set 1)"),
            cet6.Id,
            1
        );

        dbContext.Albums.Add(album2016June1);
        
        var episode = new Episode(
            new MultilingualString("完整听力", "Full Listening"),
            album2016June1.Id,
            "/audios/CET6/2016/2016.6.1.mp3",
            0, 
            "", 
            1
        );

        dbContext.Episodes.Add(episode);

        await dbContext.SaveChangesAsync();
    }
}
