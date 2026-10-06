using System.Text.Json.Serialization;
using KaoyanService.Domain.Entity;
using KaoyanService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaoyanService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Route("/api/kaoyan/[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly KaoyanDbContext db;

    public AdminController(KaoyanDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPapers()
    {
        var papers = await db.ExamPapers.AsNoTracking()
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.Series)
            .Select(p => new
            {
                p.Id,
                p.Year,
                p.Series,
                seriesName = p.Series == ExamPaper.SeriesEnglish1 ? "英语一" : "英语二",
                p.Title,
                p.IsVisible,
                sectionCount = db.ExamSections.Count(s => s.PaperId == p.Id),
                questionCount = (
                    from s in db.ExamSections
                    join q in db.ExamQuestions on s.Id equals q.SectionId
                    where s.PaperId == p.Id
                    select q.Id).Count()
            })
            .ToListAsync();
        return Ok(papers);
    }

    [HttpGet]
    public async Task<ActionResult> GetPaperFull([FromQuery] Guid paperId)
    {
        var paper = await db.ExamPapers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paperId);
        if (paper == null)
            return NotFound("试卷不存在");

        var sections = await db.ExamSections.AsNoTracking()
            .Where(s => s.PaperId == paperId)
            .OrderBy(s => s.SequenceNumber)
            .ToListAsync();
        var sectionIds = sections.Select(s => s.Id).ToArray();
        var questions = await db.ExamQuestions.AsNoTracking()
            .Where(q => sectionIds.Contains(q.SectionId))
            .OrderBy(q => q.SequenceNumber)
            .ThenBy(q => q.Number)
            .ToListAsync();
        var qBySection = questions.GroupBy(q => q.SectionId).ToDictionary(g => g.Key, g => g.ToList());

        return Ok(new
        {
            paper.Id,
            paper.Year,
            paper.Series,
            paper.Title,
            paper.IsVisible,
            sections = sections.Select(s =>
            {
                qBySection.TryGetValue(s.Id, out var qs);
                qs ??= new List<ExamQuestion>();
                return new
                {
                    s.Id,
                    s.SectionType,
                    s.Title,
                    s.Passage,
                    s.SequenceNumber,
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
            })
        });
    }

    [HttpPost]
    public async Task<ActionResult> TogglePaperVisibility([FromBody] IdRequest request)
    {
        var paper = await db.ExamPapers.FindAsync(request.Id);
        if (paper == null)
            return NotFound("试卷不存在");
        if (paper.IsVisible) paper.Hide();
        else paper.Show();
        await db.SaveChangesAsync();
        return Ok(new { message = paper.IsVisible ? "已显示" : "已隐藏", isVisible = paper.IsVisible });
    }

    /// <summary>
    /// 新增空试卷（自动带完形 + 阅读两个空分区）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreatePaper([FromBody] CreatePaperRequest request)
    {
        if (request.Year < 2000 || request.Year > 2100)
            return BadRequest("年份无效");

        var series = ExamPaper.NormalizeSeries(request.Series ?? ExamPaper.SeriesEnglish1);
        var exists = await db.ExamPapers.AnyAsync(p => p.Year == request.Year && p.Series == series);
        if (exists)
            return BadRequest($"已存在 {request.Year} 年{ExamPaper.SeriesDisplayName(series)}");

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"{request.Year}年考研{ExamPaper.SeriesDisplayName(series)}（模拟）"
            : request.Title!.Trim();

        var paper = new ExamPaper(request.Year, series, title, 0);
        db.ExamPapers.Add(paper);
        db.ExamSections.Add(new ExamSection(paper.Id, ExamSection.TypeCloze, "完形填空", "", 1));
        db.ExamSections.Add(new ExamSection(paper.Id, ExamSection.TypeReading, "阅读理解", "", 2));
        await db.SaveChangesAsync();
        return Ok(new { message = "已创建", paperId = paper.Id });
    }

    /// <summary>
    /// 删除试卷及其分区、题目
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> DeletePaper([FromBody] IdRequest request)
    {
        var paper = await db.ExamPapers.FindAsync(request.Id);
        if (paper == null)
            return NotFound("试卷不存在");

        var sections = await db.ExamSections.Where(s => s.PaperId == paper.Id).ToListAsync();
        var sectionIds = sections.Select(s => s.Id).ToArray();
        var questions = await db.ExamQuestions.Where(q => sectionIds.Contains(q.SectionId)).ToListAsync();
        db.ExamQuestions.RemoveRange(questions);
        db.ExamSections.RemoveRange(sections);
        db.ExamPapers.Remove(paper);
        await db.SaveChangesAsync();
        return Ok(new { message = "已删除" });
    }

    /// <summary>
    /// 按 paperId 覆盖保存分区与题目（表单编辑用）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> SavePaperContent([FromBody] SavePaperContentRequest request)
    {
        if (request.PaperId == Guid.Empty)
            return BadRequest("paperId 无效");

        var paper = await db.ExamPapers.FindAsync(request.PaperId);
        if (paper == null)
            return NotFound("试卷不存在");

        if (!string.IsNullOrWhiteSpace(request.Title))
            paper.ChangeTitle(request.Title!);

        var oldSections = await db.ExamSections.Where(s => s.PaperId == paper.Id).ToListAsync();
        var oldSectionIds = oldSections.Select(s => s.Id).ToArray();
        var oldQuestions = await db.ExamQuestions.Where(q => oldSectionIds.Contains(q.SectionId)).ToListAsync();
        db.ExamQuestions.RemoveRange(oldQuestions);
        db.ExamSections.RemoveRange(oldSections);

        var sections = request.Sections ?? Array.Empty<ImportSectionItem>();
        var seq = 1;
        foreach (var sec in sections)
        {
            var section = new ExamSection(
                paper.Id,
                sec.SectionType ?? ExamSection.TypeCloze,
                string.IsNullOrWhiteSpace(sec.Title)
                    ? (ExamSection.NormalizeType(sec.SectionType ?? "") == ExamSection.TypeReading ? "阅读理解" : "完形填空")
                    : sec.Title!,
                sec.Passage ?? string.Empty,
                sec.SequenceNumber <= 0 ? seq : sec.SequenceNumber);
            db.ExamSections.Add(section);

            var qSeq = 1;
            foreach (var q in sec.Questions ?? Array.Empty<ImportQuestionItem>())
            {
                try
                {
                    var options = (q.Options ?? Array.Empty<string>())
                        .Select(o => (o ?? string.Empty).Trim())
                        .Where(o => o.Length > 0)
                        .ToArray();
                    db.ExamQuestions.Add(new ExamQuestion(
                        section.Id,
                        q.Number <= 0 ? qSeq : q.Number,
                        q.Stem ?? string.Empty,
                        options,
                        q.CorrectAnswer,
                        q.SequenceNumber <= 0 ? qSeq : q.SequenceNumber,
                        q.Explanation));
                }
                catch (Exception ex)
                {
                    return BadRequest($"分区「{section.Title}」第 {qSeq} 题无效：{ex.Message}");
                }
                qSeq++;
            }
            seq++;
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "保存成功", paperId = paper.Id });
    }

    /// <summary>
    /// 导入/覆盖一整套试卷内容（分区 + 题目 + 答案）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> ImportPaper([FromBody] ImportPaperRequest request)
    {
        if (request.Year < 2000 || request.Year > 2100)
            return BadRequest("年份无效");

        var series = ExamPaper.NormalizeSeries(request.Series);
        var paper = await db.ExamPapers.FirstOrDefaultAsync(p => p.Year == request.Year && p.Series == series);
        if (paper == null)
        {
            paper = new ExamPaper(
                request.Year,
                series,
                string.IsNullOrWhiteSpace(request.Title)
                    ? $"{request.Year}年考研{ExamPaper.SeriesDisplayName(series)}"
                    : request.Title!,
                0);
            db.ExamPapers.Add(paper);
        }
        else if (!string.IsNullOrWhiteSpace(request.Title))
        {
            paper.ChangeTitle(request.Title!);
        }

        await db.SaveChangesAsync();

        var oldSections = await db.ExamSections.Where(s => s.PaperId == paper.Id).ToListAsync();
        var oldSectionIds = oldSections.Select(s => s.Id).ToArray();
        var oldQuestions = await db.ExamQuestions.Where(q => oldSectionIds.Contains(q.SectionId)).ToListAsync();
        db.ExamQuestions.RemoveRange(oldQuestions);
        db.ExamSections.RemoveRange(oldSections);

        var sections = request.Sections ?? Array.Empty<ImportSectionItem>();
        if (sections.Length == 0)
            return BadRequest("至少需要一个分区（完形或阅读）");

        var seq = 1;
        foreach (var sec in sections)
        {
            var section = new ExamSection(
                paper.Id,
                sec.SectionType ?? ExamSection.TypeCloze,
                string.IsNullOrWhiteSpace(sec.Title)
                    ? (ExamSection.NormalizeType(sec.SectionType ?? "") == ExamSection.TypeReading ? "阅读理解" : "完形填空")
                    : sec.Title!,
                sec.Passage ?? string.Empty,
                sec.SequenceNumber <= 0 ? seq : sec.SequenceNumber);
            db.ExamSections.Add(section);

            var qSeq = 1;
            foreach (var q in sec.Questions ?? Array.Empty<ImportQuestionItem>())
            {
                try
                {
                    db.ExamQuestions.Add(new ExamQuestion(
                        section.Id,
                        q.Number <= 0 ? qSeq : q.Number,
                        q.Stem ?? string.Empty,
                        q.Options ?? Array.Empty<string>(),
                        q.CorrectAnswer,
                        q.SequenceNumber <= 0 ? qSeq : q.SequenceNumber,
                        q.Explanation));
                }
                catch (Exception ex)
                {
                    return BadRequest($"分区「{section.Title}」第 {qSeq} 题无效：{ex.Message}");
                }
                qSeq++;
            }
            seq++;
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "导入成功", paperId = paper.Id });
    }
}

public class IdRequest
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}

public class CreatePaperRequest
{
    public int Year { get; set; }
    public string? Series { get; set; }
    public string? Title { get; set; }
}

public class SavePaperContentRequest
{
    public Guid PaperId { get; set; }
    public string? Title { get; set; }
    public ImportSectionItem[]? Sections { get; set; }
}

public class ImportPaperRequest
{
    public int Year { get; set; }
    public string? Series { get; set; }
    public string? Title { get; set; }
    public ImportSectionItem[]? Sections { get; set; }
}

public class ImportSectionItem
{
    public string? SectionType { get; set; }
    public string? Title { get; set; }
    public string? Passage { get; set; }
    public int SequenceNumber { get; set; }
    public ImportQuestionItem[]? Questions { get; set; }
}

public class ImportQuestionItem
{
    public int Number { get; set; }
    public string? Stem { get; set; }
    public string[]? Options { get; set; }
    public int CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public int SequenceNumber { get; set; }
}
