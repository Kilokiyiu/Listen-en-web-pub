using System.ComponentModel.DataAnnotations;
using ListenService.Domain;
using ListenService.WebAPI.Controllers.Listen.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ListenService.WebAPI.Controllers.Listen;

[ApiController]
[Route("[controller]/[action]")]
[Route("/api/listen/[controller]/[action]")]
public class ListenController : ControllerBase
{
    private readonly IListenRepo repo;

    public ListenController(IListenRepo repo)
    {
        this.repo = repo;
    }

    /// <summary>
    /// 获取所有的分类
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<CategoryPesponse[]>> GetCategories()
    {
        var categories = await repo.GetAllCategoriesAsync();
        return categories.Select(e => new CategoryPesponse
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            SequenceNumber = e.SequenceNumber,
            CoverUrl = e.CoverUrl
        }).ToArray();
    }

    /// <summary>
    /// 获取分类下的所有试卷
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<AlbumResponse[]>> GetAlbumsByCategoryId([Required] Guid categoryId)
    {
        var albums = await repo.GetAllAlbumAsync(categoryId);
        return albums.Select(e => new AlbumResponse
        {
            Id = e.Id,
            Name = e.Name,
            CategoryId = e.CategoryId,
            SequenceNumber = e.SequenceNumber,
            PaperFileUrl = e.PaperFileUrl,
            AnswerFileUrl = e.AnswerFileUrl
        }).ToArray();
    }

    /// <summary>
    /// 获取试卷详情（含试卷/答案下载地址）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AlbumResponse>> GetAlbumById([Required] Guid albumId)
    {
        var album = await repo.GetAlbumByIdAsync(albumId);
        if (album == null)
        {
            return NotFound("试卷不存在");
        }

        return new AlbumResponse
        {
            Id = album.Id,
            Name = album.Name,
            CategoryId = album.CategoryId,
            SequenceNumber = album.SequenceNumber,
            PaperFileUrl = album.PaperFileUrl,
            AnswerFileUrl = album.AnswerFileUrl
        };
    }

    /// <summary>
    /// 获取一张试卷的所有题目
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<EpisodeResponse[]>> GetEpisodesByAlbumId([Required] Guid albumId)
    {
        var episodes = await repo.GetAllEpisodesAsync(albumId);
        return episodes.Select(e => new EpisodeResponse
        {
            Id = e.Id,
            Name = e.Name,
            AlbumId = e.AlbumId,
            AudioUrl = e.AudioUrl,
            DurationInSecond = e.DurationInSecond,
            SubtitleType = e.SubtitleType,
            Subtitle = e.Subtitle
        }).ToArray();
    }

    /// <summary>
    /// 获取详细的题目信息
    /// </summary>
    /// <param name="episodeId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<EpisodeDetailResponse>> GetDetailEpisodesByEpisodeId([Required] Guid episodeId)
    {
        var episode = await repo.GetEpisodeByIdAsync(episodeId);
        if (episode == null)
        {
            return NotFound("题目不存在");
        }
        return new EpisodeDetailResponse
        {
            Id = episode.Id,
            Name = episode.Name,
            AlbumId = episode.AlbumId,
            AudioUrl = episode.AudioUrl,
            DurationInSecond = episode.DurationInSecond,
            Subtitle = episode.Subtitle,
            SubtitleType = episode.SubtitleType
        };
    }

    /// <summary>
    /// 获取听力做题分区与单选题（不含答案）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<QuizPaperDto>> GetQuizByAlbumId([Required] Guid albumId)
    {
        var album = await repo.GetAlbumByIdAsync(albumId);
        if (album == null)
            return NotFound("试卷不存在");

        var sections = await repo.GetQuizSectionsByAlbumIdAsync(albumId);
        var questions = await repo.GetQuizQuestionsBySectionIdsAsync(sections.Select(s => s.Id));
        var qBySection = questions.GroupBy(q => q.SectionId).ToDictionary(g => g.Key, g => g.ToArray());

        var sectionDtos = sections.Select(s =>
        {
            qBySection.TryGetValue(s.Id, out var qs);
            qs ??= Array.Empty<Domain.Entity.QuizQuestion>();
            return new QuizSectionPublicDto
            {
                Id = s.Id,
                GroupName = s.GroupName,
                Title = s.Title,
                Transcript = s.Transcript,
                AudioUrl = s.AudioUrl,
                SequenceNumber = s.SequenceNumber,
                QuestionCount = qs.Length,
                Questions = qs.Select(q => new QuizQuestionPublicDto
                {
                    Id = q.Id,
                    Number = q.Number,
                    Stem = q.Stem,
                    Options = q.GetOptions(),
                    SequenceNumber = q.SequenceNumber
                }).ToArray()
            };
        }).ToArray();

        return new QuizPaperDto
        {
            AlbumId = albumId,
            SectionCount = sectionDtos.Length,
            QuestionCount = sectionDtos.Sum(s => s.QuestionCount),
            Sections = sectionDtos
        };
    }

    /// <summary>
    /// 提交听力答题（可按分区）并返回判分与正确答案
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SubmitQuizResponse>> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        if (request.AlbumId == Guid.Empty)
            return BadRequest("albumId 无效");

        var album = await repo.GetAlbumByIdAsync(request.AlbumId);
        if (album == null)
            return NotFound("试卷不存在");

        var sections = await repo.GetQuizSectionsByAlbumIdAsync(request.AlbumId);
        if (request.SectionId.HasValue && request.SectionId != Guid.Empty)
            sections = sections.Where(s => s.Id == request.SectionId.Value).ToArray();

        var questions = await repo.GetQuizQuestionsBySectionIdsAsync(sections.Select(s => s.Id));
        var answerMap = (request.Answers ?? Array.Empty<QuizAnswerItem>())
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Last().SelectedIndex);

        var results = questions.Select(q =>
        {
            answerMap.TryGetValue(q.Id, out var selected);
            var hasAnswer = answerMap.ContainsKey(q.Id);
            var isCorrect = hasAnswer && selected == q.CorrectAnswer;
            return new QuizQuestionResultDto
            {
                QuestionId = q.Id,
                SectionId = q.SectionId,
                Number = q.Number,
                Stem = q.Stem,
                Options = q.GetOptions(),
                SelectedIndex = hasAnswer ? selected : null,
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                Explanation = q.Explanation
            };
        }).ToArray();

        var correctCount = results.Count(r => r.IsCorrect);
        var total = results.Length;
        return new SubmitQuizResponse
        {
            AlbumId = request.AlbumId,
            SectionId = request.SectionId,
            Total = total,
            CorrectCount = correctCount,
            ScorePercent = total == 0 ? 0 : Math.Round(correctCount * 100.0 / total, 1),
            Results = results
        };
    }
}
