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
[Route("/api/listen/[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IListenRepo repo;
    private readonly ListenDbContext dbContext;
    private readonly IWebHostEnvironment env;
    private readonly ListenCacheInvalidator cacheInvalidator;
    
    public AdminController(
        IListenRepo repo,
        ListenDbContext dbContext,
        IWebHostEnvironment env,
        ListenCacheInvalidator cacheInvalidator)
    {
        this.repo = repo;
        this.dbContext = dbContext;
        this.env = env;
        this.cacheInvalidator = cacheInvalidator;
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

        await cacheInvalidator.InvalidateAlbumAsync(album.Id, category.Id);
        await cacheInvalidator.InvalidateCategoriesAsync();
        
        return Ok(new { episode.Id, audioUrl });
    }

    /// <summary>
    /// 上传试卷 PDF 或答案 PDF（按 Album 维度，一套听力一份）
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult> UploadAlbumDocument(
        [FromForm] Guid albumId,
        [FromForm] string documentType,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("请选择 PDF 文件");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf")
            return BadRequest("只支持 PDF 格式");

        var type = documentType?.Trim().ToLowerInvariant();
        if (type is not ("paper" or "answer"))
            return BadRequest("documentType 须为 paper（试卷）或 answer（答案）");

        var album = await dbContext.Albums.FindAsync(albumId);
        if (album == null)
            return NotFound("试卷不存在");

        var category = await dbContext.Categories.FindAsync(album.CategoryId);
        var categoryDir = (category?.Code ?? "misc").ToUpperInvariant();
        var fileName = type == "paper" ? $"{albumId}.paper.pdf" : $"{albumId}.answer.pdf";
        var dirPath = Path.Combine(env.WebRootPath, "papers", categoryDir);
        Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, fileName);
        var relativeUrl = $"/papers/{categoryDir}/{fileName}";

        if (type == "paper")
            TryDeleteWebFile(album.PaperFileUrl);
        else
            TryDeleteWebFile(album.AnswerFileUrl);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (type == "paper")
            album.SetPaperFileUrl(relativeUrl);
        else
            album.SetAnswerFileUrl(relativeUrl);

        await dbContext.SaveChangesAsync();
        await cacheInvalidator.InvalidateAlbumAsync(album.Id, album.CategoryId);
        return Ok(new { url = relativeUrl, documentType = type });
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

        var album = await dbContext.Albums.FindAsync(episode.AlbumId);
        if (album != null)
        {
            await cacheInvalidator.InvalidateEpisodeAsync(episode.Id, album.Id, album.CategoryId);
        }
        
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

        var album = await dbContext.Albums.FindAsync(episode.AlbumId);
        if (album != null)
        {
            await cacheInvalidator.InvalidateEpisodeAsync(episode.Id, album.Id, album.CategoryId);
        }

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
                QuizSectionCount = dbContext.QuizSections.Count(s => s.AlbumId == a.Id),
                QuizQuestionCount = (
                    from s in dbContext.QuizSections
                    join q in dbContext.QuizQuestions on s.Id equals q.SectionId
                    where s.AlbumId == a.Id
                    select q.Id).Count(),
                FirstEpisodeId = firstEpisode != null ? firstEpisode.Id : Guid.Empty,
                Subtitle = firstEpisode != null ? firstEpisode.Subtitle : null,
                HasSubtitle = firstEpisode != null && !string.IsNullOrWhiteSpace(firstEpisode.Subtitle),
                a.PaperFileUrl,
                a.AnswerFileUrl,
                HasPaper = !string.IsNullOrWhiteSpace(a.PaperFileUrl),
                HasAnswer = !string.IsNullOrWhiteSpace(a.AnswerFileUrl)
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
        await cacheInvalidator.InvalidateAlbumAsync(album.Id, album.CategoryId);
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

        var albumId = episode.AlbumId;
        var album = await dbContext.Albums.FindAsync(albumId);

        dbContext.Episodes.Remove(episode);
        await dbContext.SaveChangesAsync();

        if (album != null)
        {
            await cacheInvalidator.InvalidateEpisodeAsync(request.EpisodeId, albumId, album.CategoryId);
        }

        return Ok(new { message = "删除成功" });
    }

    /// <summary>
    /// 获取某套听力试卷的做题分区（含题目与答案）
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetQuizSections([FromQuery] Guid albumId)
    {
        if (albumId == Guid.Empty)
            return BadRequest("albumId 无效");

        var sections = await dbContext.QuizSections.AsNoTracking()
            .Where(s => s.AlbumId == albumId)
            .OrderBy(s => s.SequenceNumber)
            .ToListAsync();
        var sectionIds = sections.Select(s => s.Id).ToArray();
        var questions = await dbContext.QuizQuestions.AsNoTracking()
            .Where(q => sectionIds.Contains(q.SectionId))
            .OrderBy(q => q.SequenceNumber)
            .ThenBy(q => q.Number)
            .ToListAsync();
        var qBySection = questions.GroupBy(q => q.SectionId).ToDictionary(g => g.Key, g => g.ToList());

        return Ok(sections.Select(s =>
        {
            qBySection.TryGetValue(s.Id, out var qs);
            qs ??= new List<QuizQuestion>();
            return new
            {
                s.Id,
                s.AlbumId,
                s.GroupName,
                s.Title,
                s.Transcript,
                s.AudioUrl,
                s.SequenceNumber,
                s.IsVisible,
                questions = qs.Select(q => new
                {
                    q.Id,
                    q.Number,
                    q.Stem,
                    options = q.GetOptions(),
                    q.CorrectAnswer,
                    q.Explanation,
                    q.SequenceNumber
                })
            };
        }));
    }

    /// <summary>
    /// 全量覆盖保存听力做题分区与题目
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SaveQuizSections([FromBody] SaveQuizSectionsRequest request)
    {
        if (request.AlbumId == Guid.Empty)
            return BadRequest("albumId 无效");

        var album = await dbContext.Albums.FindAsync(request.AlbumId);
        if (album == null)
            return NotFound("试卷不存在");

        var oldSections = await dbContext.QuizSections.Where(s => s.AlbumId == request.AlbumId).ToListAsync();
        var oldIds = oldSections.Select(s => s.Id).ToArray();
        var oldQuestions = await dbContext.QuizQuestions.Where(q => oldIds.Contains(q.SectionId)).ToListAsync();
        // 保留已有音频路径：按序号匹配回填
        var oldAudioBySeq = oldSections.ToDictionary(s => s.SequenceNumber, s => s.AudioUrl);

        dbContext.QuizQuestions.RemoveRange(oldQuestions);
        dbContext.QuizSections.RemoveRange(oldSections);

        var sections = request.Sections ?? Array.Empty<QuizSectionUpsertItem>();
        var secSeq = 1;
        foreach (var sec in sections)
        {
            var seq = sec.SequenceNumber <= 0 ? secSeq : sec.SequenceNumber;
            var audioUrl = !string.IsNullOrWhiteSpace(sec.AudioUrl)
                ? sec.AudioUrl
                : (oldAudioBySeq.TryGetValue(seq, out var kept) ? kept : null);

            var section = new QuizSection(
                request.AlbumId,
                sec.Title ?? $"Passage {secSeq}",
                sec.Transcript ?? string.Empty,
                audioUrl,
                seq,
                QuizSection.InferGroupName(sec.GroupName, sec.Title));
            if (sec.IsVisible == false)
                section.Hide();
            dbContext.QuizSections.Add(section);

            var qSeq = 1;
            foreach (var item in sec.Questions ?? Array.Empty<QuizQuestionUpsertItem>())
            {
                try
                {
                    var options = (item.Options ?? Array.Empty<string>())
                        .Select(o => (o ?? string.Empty).Trim())
                        .Where(o => o.Length > 0)
                        .ToArray();
                    dbContext.QuizQuestions.Add(new QuizQuestion(
                        section.Id,
                        item.Number <= 0 ? qSeq : item.Number,
                        item.Stem ?? string.Empty,
                        options,
                        item.CorrectAnswer,
                        item.SequenceNumber <= 0 ? qSeq : item.SequenceNumber,
                        item.Explanation));
                }
                catch (Exception ex)
                {
                    return BadRequest($"分区「{section.Title}」第 {qSeq} 题无效：{ex.Message}");
                }
                qSeq++;
            }
            secSeq++;
        }

        await dbContext.SaveChangesAsync();
        return Ok(new { message = "保存成功", sectionCount = sections.Length });
    }

    /// <summary>
    /// 调整某段材料所属 Section（A/B/C），用于录错分区时快速纠错。
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> MoveQuizSectionGroup([FromBody] MoveQuizSectionGroupRequest request)
    {
        if (request.SectionId == Guid.Empty)
            return BadRequest("sectionId 无效");

        var section = await dbContext.QuizSections.FindAsync(request.SectionId);
        if (section == null)
            return NotFound("材料不存在");

        var before = section.GroupName;
        section.SetGroupName(request.GroupName);
        await dbContext.SaveChangesAsync();
        return Ok(new
        {
            message = $"已从 {before} 调整到 {section.GroupName}",
            sectionId = section.Id,
            groupName = section.GroupName,
            title = section.Title
        });
    }

    /// <summary>
    /// 上传某分区听力音频（按 sectionId）
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UploadSectionAudio(
        [FromForm] Guid sectionId,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("请选择音频文件");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".mp3" or ".wav" or ".m4a"))
            return BadRequest("只支持 MP3/wav/m4a");

        var section = await dbContext.QuizSections.FindAsync(sectionId);
        if (section == null)
            return NotFound("分区不存在");

        var album = await dbContext.Albums.FindAsync(section.AlbumId);
        var category = album == null ? null : await dbContext.Categories.FindAsync(album.CategoryId);
        var categoryDir = (category?.Code ?? "misc").ToUpperInvariant();
        var fileName = $"{sectionId}{ext}";
        var dirPath = Path.Combine(env.WebRootPath, "audios", categoryDir, "sections");
        Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, fileName);
        var relativeUrl = $"/audios/{categoryDir}/sections/{fileName}";

        TryDeleteWebFile(section.AudioUrl);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        section.SetAudio(relativeUrl);
        await dbContext.SaveChangesAsync();
        return Ok(new { url = relativeUrl, sectionId });
    }

    private void TryDeleteWebFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return;
        var filePath = Path.Combine(env.WebRootPath, relativeUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
}

public record UpdateSubtitleRequest(
    [property: JsonPropertyName("episodeId")] Guid EpisodeId,
    string Subtitle,
    string? SubtitleType);

public record ToggleVisibilityRequest([property: JsonPropertyName("episodeId")] Guid EpisodeId);

public record DeleteEpisodeRequest([property: JsonPropertyName("episodeId")] Guid EpisodeId);

public class SaveQuizSectionsRequest
{
    [JsonPropertyName("albumId")]
    public Guid AlbumId { get; set; }

    [JsonPropertyName("sections")]
    public QuizSectionUpsertItem[]? Sections { get; set; }
}

public class QuizSectionUpsertItem
{
    [JsonPropertyName("groupName")]
    public string? GroupName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("audioUrl")]
    public string? AudioUrl { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public int SequenceNumber { get; set; }

    [JsonPropertyName("isVisible")]
    public bool? IsVisible { get; set; }

    [JsonPropertyName("questions")]
    public QuizQuestionUpsertItem[]? Questions { get; set; }
}

public class QuizQuestionUpsertItem
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("stem")]
    public string? Stem { get; set; }

    [JsonPropertyName("options")]
    public string[]? Options { get; set; }

    [JsonPropertyName("correctAnswer")]
    public int CorrectAnswer { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public int SequenceNumber { get; set; }
}

public class MoveQuizSectionGroupRequest
{
    [JsonPropertyName("sectionId")]
    public Guid SectionId { get; set; }

    [JsonPropertyName("groupName")]
    public string? GroupName { get; set; }
}