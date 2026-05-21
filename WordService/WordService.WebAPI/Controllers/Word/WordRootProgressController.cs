using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("word-root-progress")]
public class WordRootProgressController : ControllerBase
{
    private readonly WordDbContext _context;

    public WordRootProgressController(WordDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取用户的词根学习进度
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserProgressDto>> GetProgress(
        [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        var masteredIds = await _context.UserWordRootProgresses
            .Where(x => x.UserId == userId && x.IsMastered)
            .Select(x => x.WordRootId)
            .ToListAsync();

        var totalRoots = await _context.WordRoots.CountAsync();
        var masteredCount = masteredIds.Count;

        // 获取最近学习的词根
        var recentProgress = await _context.UserWordRootProgresses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreationTime)
            .Take(10)
            .ToListAsync();

        return Ok(new UserProgressDto
        {
            TotalRoots = totalRoots,
            MasteredCount = masteredCount,
            MasteredIds = masteredIds,
            RecentMasteredRoots = recentProgress
                .Where(x => x.IsMastered)
                .Select(x => x.WordRootId)
                .ToList()
        });
    }

    /// <summary>
    /// 标记词根为已掌握
    /// </summary>
    [HttpPost("{wordRootId:guid}/master")]
    public async Task<ActionResult> MarkAsMastered(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid wordRootId)
    {
        // 检查是否已存在
        var existing = await _context.UserWordRootProgresses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.WordRootId == wordRootId);

        if (existing != null)
        {
            if (existing.IsMastered)
                return BadRequest("该词根已掌握");
            existing.MarkAsMastered();
        }
        else
        {
            var progress = new Domain.Entity.UserWordRootProgress(userId, wordRootId);
            progress.MarkAsMastered();
            _context.UserWordRootProgresses.Add(progress);
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// 获取下一个待学习的词根
    /// </summary>
    [HttpGet("next")]
    public async Task<ActionResult<Guid?>> GetNextRoot(
        [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        var learnedIds = await _context.UserWordRootProgresses
            .Where(x => x.UserId == userId)
            .Select(x => x.WordRootId)
            .ToListAsync();

        var nextRoot = await _context.WordRoots
            .Where(x => !learnedIds.Contains(x.Id))
            .OrderBy(x => x.RootId)
            .FirstOrDefaultAsync();

        return Ok(nextRoot?.Id);
    }
}

public class UserProgressDto
{
    public int TotalRoots { get; set; }
    public int MasteredCount { get; set; }
    public List<Guid> MasteredIds { get; set; } = new();
    public List<Guid> RecentMasteredRoots { get; set; } = new();
}
