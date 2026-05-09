using System.Text.Json.Serialization;
using DomainCommons;
using ListenService.Domain;
using ListenService.Domain.Entity;
using ListenService.Infrastrucure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ListenService.WebAPI.Controllers.Admin;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IListenRepo repo;
    private readonly ListenDbContext dbContext;
    private readonly IWebHostEnvironment env;
    
    public AdminController(IListenRepo repo, ListenDbContext dbContext, IWebHostEnvironment env)
    {
        this.repo = repo;
        this.dbContext = dbContext;
        this.env = env;
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UploadAudio(
        [FromForm] string categoryParam,
        [FromForm] int year,
        [FromForm] int month,
        [FromForm] int setNumber,
        [FromForm] IFormFile file,
        [FromForm] string? subtitle = "")
    {
        if (string.IsNullOrWhiteSpace(categoryParam))
            return BadRequest("参数 categoryParam 不能为空");
        if (file == null || file.Length == 0) return BadRequest("请选择正确的音频文件");
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".mp3" && ext != ".wav" && ext != ".m4a") return BadRequest("只支持MP3/wav/m4a格式");
        
        var categoryCode = categoryParam.ToLower(); // 统一转小写，与数据库中Code一致
        var categoryDirName = categoryParam.ToUpper(); // 目录名保持大写，如CET6
        var category = await repo.FindCategoryByCodeAsync(categoryCode);
        if (category == null)
        {
            category = new Category(
                new MultilingualString(
                    categoryParam.ToUpper() == "CET4" ? "大学英语四级" : "大学英语六级",
                    categoryParam.ToUpper() == "CET4" ? "CET-4" : "CET-6"),
                categoryCode,
                categoryParam.ToUpper() == "CET4" ? 1 : 2,
                $"/images/{categoryCode}.png");
            dbContext.Categories.Add(category);
        }

        string fileName = $"{year}.{month}.{setNumber}.mp3";
        string dirPath = Path.Combine(env.WebRootPath, "audios", categoryDirName, year.ToString());
        Directory.CreateDirectory(dirPath);
        string filePath = Path.Combine(dirPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string audioUrl = $"/audios/{categoryDirName}/{year}/{fileName}";

        string albumNameCn = $"{year}年{month}月大学英语{(categoryParam.ToUpper() == "CET4" ? "四级" : "六级")}听力真题（第{setNumber}套）";
        string albumNameEn = $"{categoryParam.ToUpper()} {(month == 6 ? "June" : "December")} {year} (Set {setNumber})";

        var album = await repo.FindAlbumByCategoryAndNameAsync(category.Id, albumNameCn);
        if (album == null)
        {
            int maxSeq = await repo.GetMaxAlbumSequenceAsync(category.Id);
            album = new Album(
                new MultilingualString(albumNameCn, albumNameEn),
                category.Id,
                maxSeq + 1);
            dbContext.Albums.Add(album);
        }
        
        int episodeSeq = await dbContext.Episodes
            .Where(e => e.AlbumId == album.Id)
            .CountAsync() + 1;
        var episode = new Episode(
            new MultilingualString("完整听力", "Full Listening"),
            album.Id,
            audioUrl,
            0,
            subtitle ?? "",
            episodeSeq,
            "json");
        dbContext.Episodes.Add(episode);
        
        await dbContext.SaveChangesAsync();
        
        return Ok(new { episode.Id, audioUrl });
    }

    /// <summary>
    /// 更新题目字幕（原文）
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateEpisodeSubtitle([FromBody] UpdateSubtitleRequest request)
    {
        var episode = await dbContext.Episodes.FindAsync(request.EpisodeId);
        if (episode == null)
        {
            return NotFound("题目不存在");
        }

        episode.ChangeSubtitle(request.Subtitle, request.SubtitleType ?? "json");
        await dbContext.SaveChangesAsync();
        
        return Ok(new { message = "原文更新成功" });
    }

    /// <summary>
    /// 获取所有题目（用于管理列表）
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetAllEpisodes() 
    {
        var episodes = await (
            from e in dbContext.Episodes
            join a in dbContext.Albums on e.AlbumId equals a.Id
            join c in dbContext.Categories on a.CategoryId equals c.Id
            orderby e.CreationTime descending
            select new
            {
                e.Id,
                NameChinese = e.Name.Chinese,
                NameEnglish = e.Name.English,
                AlbumNameChinese = a.Name.Chinese,
                AlbumNameEnglish = a.Name.English,
                CategoryNameChinese = c.Name.Chinese,
                CategoryNameEnglish = c.Name.English,
                e.AudioUrl,
                e.DurationInSecond,
                e.Subtitle,
                e.SubtitleType,
                e.IsVisible,
                e.CreationTime
            }
        ).ToListAsync();

        return Ok(episodes);
    }

    /// <summary>
    /// 是否隐藏题目
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ToggleEpisodeVisibility([FromBody] ToggleVisibilityRequest request)
    {
        var episode = await dbContext.Episodes.FindAsync(request.EpisodeId);
        if (episode == null)
            return NotFound("题目不存在");

        if (episode.IsVisible)
            episode.Hide();
        else
            episode.Show();

        await dbContext.SaveChangesAsync();
        return Ok(new { message = episode.IsVisible ? "已显示" : "已隐藏" });
    }

    /// <summary>
    /// 在管理员页面获取所有试卷
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetAllAlbums()
    {
        var albums = await (
            from a in dbContext.Albums
            join c in dbContext.Categories on a.CategoryId equals c.Id
            let firstEpisode = dbContext.Episodes.Where(e => e.AlbumId == a.Id).OrderBy(e => e.SequenceNumber).FirstOrDefault()
            orderby c.SequenceNumber, a.SequenceNumber
            select new
            {
                a.Id,
                NameChinese = a.Name.Chinese,
                NameEnglish = a.Name.English,
                CategoryNameChinese = c.Name.Chinese,
                CategoryNameEnglish = c.Name.English,
                a.IsVisible,
                a.CreationTime,
                EpisodeCount = dbContext.Episodes.Count(e => e.AlbumId == a.Id),
                FirstEpisodeId = firstEpisode != null ? firstEpisode.Id : Guid.Empty,
                Subtitle = firstEpisode != null ? firstEpisode.Subtitle : null,
                HasSubtitle = firstEpisode != null && !string.IsNullOrWhiteSpace(firstEpisode.Subtitle)
            }
        ).ToListAsync();
        return Ok(albums);
    }

    /// <summary>
    /// 是否隐藏试卷
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ToggleAlbumVisibility([FromBody] ToggleVisibilityRequest request)
    {
        var album = await dbContext.Albums.FindAsync(request.EpisodeId);
        if (album == null)
            return NotFound("试卷不存在");

        if (album.IsVisible)
            album.Hide();
        else
            album.Show();

        await dbContext.SaveChangesAsync();
        return Ok(new { message = album.IsVisible ? "已显示" : "已隐藏" });
    }

    /// <summary>
    /// 删除题目（同时删除关联的音频文件）
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteEpisode([FromBody] DeleteEpisodeRequest request)
    {
        var episode = await dbContext.Episodes.FindAsync(request.EpisodeId);
        if (episode == null)
            return NotFound("题目不存在");
        
        if (!string.IsNullOrEmpty(episode.AudioUrl))
        {
            string filePath = Path.Combine(env.WebRootPath, episode.AudioUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        dbContext.Episodes.Remove(episode);
        await dbContext.SaveChangesAsync();
        return Ok(new { message = "删除成功" });
    }
}

public record UpdateSubtitleRequest(
    [property: JsonPropertyName("episodeId")] Guid EpisodeId,
    string Subtitle,
    string? SubtitleType);

public record ToggleVisibilityRequest([property: JsonPropertyName("episodeId")] Guid EpisodeId);

public record DeleteEpisodeRequest([property: JsonPropertyName("episodeId")] Guid EpisodeId);