using System.ComponentModel.DataAnnotations;
using KaoyanService.Domain;
using KaoyanService.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaoyanService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Route("/api/kaoyan/[controller]/[action]")]
public class KaoyanController : ControllerBase
{
    private readonly IKaoyanRepo repo;

    public KaoyanController(IKaoyanRepo repo)
    {
        this.repo = repo;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "ok", service = "kaoyan" });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetModuleStatus()
    {
        return Ok(new
        {
            code = KaoyanModule.Code,
            name = KaoyanModule.Name,
            comingSoon = KaoyanModule.ComingSoon,
            description = KaoyanModule.Description,
        });
    }

    /// <summary>
    /// 历年试卷列表
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetPapers([FromQuery] string? series = null)
    {
        var papers = await repo.GetVisiblePapersAsync(series);
        return Ok(papers.Select(p => new
        {
            p.Id,
            p.Year,
            p.Series,
            seriesName = ExamPaper.SeriesDisplayName(p.Series),
            p.Title,
            p.SequenceNumber
        }));
    }

    /// <summary>
    /// 试卷详情（分区 + 题目，不含答案）
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetPaperDetail([Required] Guid paperId)
    {
        var paper = await repo.GetPaperByIdAsync(paperId);
        if (paper == null || !paper.IsVisible)
            return NotFound("试卷不存在");

        var sections = await repo.GetSectionsByPaperIdAsync(paperId);
        var questions = await repo.GetQuestionsBySectionIdsAsync(sections.Select(s => s.Id));
        var qBySection = questions.GroupBy(q => q.SectionId).ToDictionary(g => g.Key, g => g.ToArray());

        return Ok(new
        {
            paper.Id,
            paper.Year,
            paper.Series,
            seriesName = ExamPaper.SeriesDisplayName(paper.Series),
            paper.Title,
            sections = sections.Select(s =>
            {
                qBySection.TryGetValue(s.Id, out var qs);
                qs ??= Array.Empty<ExamQuestion>();
                return new
                {
                    s.Id,
                    s.SectionType,
                    s.Title,
                    s.Passage,
                    s.SequenceNumber,
                    questionCount = qs.Length,
                    questions = qs.Select(q => new
                    {
                        q.Id,
                        q.Number,
                        q.Stem,
                        options = q.GetOptions(),
                        q.SequenceNumber
                    })
                };
            })
        });
    }

    /// <summary>
    /// 单题即时判分（完形点空格选完后使用）
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CheckAnswer([FromBody] KaoyanAnswerItem request)
    {
        if (request.QuestionId == Guid.Empty)
            return BadRequest("questionId 无效");

        var q = await repo.GetQuestionByIdAsync(request.QuestionId);
        if (q == null)
            return NotFound("题目不存在");

        var isCorrect = request.SelectedIndex == q.CorrectAnswer;
        return Ok(new
        {
            questionId = q.Id,
            selectedIndex = request.SelectedIndex,
            correctAnswer = q.CorrectAnswer,
            isCorrect,
            explanation = q.Explanation,
            options = q.GetOptions()
        });
    }

    /// <summary>
    /// 提交整卷或指定分区答案并判分
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> SubmitAnswers([FromBody] SubmitKaoyanRequest request)
    {
        if (request.PaperId == Guid.Empty)
            return BadRequest("paperId 无效");

        var paper = await repo.GetPaperByIdAsync(request.PaperId);
        if (paper == null || !paper.IsVisible)
            return NotFound("试卷不存在");

        var sections = await repo.GetSectionsByPaperIdAsync(request.PaperId);
        if (request.SectionId.HasValue && request.SectionId != Guid.Empty)
            sections = sections.Where(s => s.Id == request.SectionId.Value).ToArray();

        var questions = await repo.GetQuestionsBySectionIdsAsync(sections.Select(s => s.Id));
        var answerMap = (request.Answers ?? Array.Empty<KaoyanAnswerItem>())
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Last().SelectedIndex);

        var results = questions.Select(q =>
        {
            var has = answerMap.ContainsKey(q.Id);
            answerMap.TryGetValue(q.Id, out var selected);
            return new
            {
                questionId = q.Id,
                sectionId = q.SectionId,
                number = q.Number,
                stem = q.Stem,
                options = q.GetOptions(),
                selectedIndex = has ? (int?)selected : null,
                correctAnswer = q.CorrectAnswer,
                isCorrect = has && selected == q.CorrectAnswer,
                explanation = q.Explanation
            };
        }).ToArray();

        var total = results.Length;
        var correctCount = results.Count(r => r.isCorrect);
        return Ok(new
        {
            paperId = request.PaperId,
            total,
            correctCount,
            scorePercent = total == 0 ? 0 : Math.Round(correctCount * 100.0 / total, 1),
            results
        });
    }
}

public class SubmitKaoyanRequest
{
    public Guid PaperId { get; set; }
    public Guid? SectionId { get; set; }
    public KaoyanAnswerItem[]? Answers { get; set; }
}

public class KaoyanAnswerItem
{
    public Guid QuestionId { get; set; }
    public int SelectedIndex { get; set; }
}
