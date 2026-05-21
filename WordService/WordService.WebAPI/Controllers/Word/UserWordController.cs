using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("user-words")]
public class UserWordController : ControllerBase
{
    private readonly WordDbContext _context;

    public UserWordController(WordDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取用户的单词列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageResult<UserWordDto>>> GetUserWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var query = _context.UserWords.Where(x => x.UserId == userId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(x => x.Word.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount
            })
            .ToListAsync();

        return Ok(new PageResult<UserWordDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// 添加单词
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserWordDto>> AddUserWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] AddUserWordRequest request)
    {
        // 检查是否已存在
        var exists = await _context.UserWords
            .AnyAsync(x => x.UserId == userId && x.Word.ToLower() == request.Word.ToLower());

        if (exists)
            return BadRequest("该单词已存在");

        var word = new UserWord(userId, request.Word, request.Definition, request.Example);
        _context.UserWords.Add(word);
        await _context.SaveChangesAsync();

        return Ok(new UserWordDto
        {
            Id = word.Id,
            Word = word.Word,
            Definition = word.Definition,
            Example = word.Example,
            NextReview = word.NextReview,
            Interval = word.Interval,
            RepetitionCount = word.RepetitionCount
        });
    }

    /// <summary>
    /// 删除单词
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUserWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id)
    {
        var word = await _context.UserWords
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (word == null)
            return NotFound();

        _context.UserWords.Remove(word);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// 获取待复习的单词
    /// </summary>
    [HttpGet("due")]
    public async Task<ActionResult<List<UserWordDto>>> GetDueWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] int limit = 20)
    {
        var now = DateTime.Now;
        var words = await _context.UserWords
            .Where(x => x.UserId == userId)
            .Where(x => x.NextReview == null || x.NextReview <= now)
            .OrderBy(x => x.NextReview)
            .Take(limit)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount
            })
            .ToListAsync();

        return Ok(words);
    }

    /// <summary>
    /// 自由复习：随机返回用户所有未掌握的单词
    /// </summary>
    [HttpGet("random")]
    public async Task<ActionResult<List<UserWordDto>>> GetRandomWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] int limit = 50)
    {
        var words = await _context.UserWords
            .Where(x => x.UserId == userId)
            .Where(x => x.Interval < 21) // 未掌握
            .OrderBy(x => Guid.NewGuid()) // 随机排序
            .Take(limit)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount
            })
            .ToListAsync();

        return Ok(words);
    }

    /// <summary>
    /// 复习单词（SM-2 算法）
    /// </summary>
    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<ReviewResultDto>> ReviewWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id,
        [FromBody] ReviewWordRequest request)
    {
        var word = await _context.UserWords
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (word == null)
            return NotFound();

        // SM-2 算法更新
        word.UpdateReview(request.Quality);

        // 记录复习日志
        var log = new WordReviewLog(userId, id, request.Quality);
        _context.WordReviewLogs.Add(log);

        await _context.SaveChangesAsync();

        return Ok(new ReviewResultDto
        {
            NextReview = word.NextReview,
            Interval = word.Interval,
            RepetitionCount = word.RepetitionCount,
            EaseFactor = word.EaseFactor
        });
    }

    /// <summary>
    /// 获取用户学习统计
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<UserWordStatsDto>> GetStats(
        [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        var words = await _context.UserWords.Where(x => x.UserId == userId).ToListAsync();

        var now = DateTime.Now;
        var dueCount = words.Count(x => x.NextReview == null || x.NextReview <= now);
        var masteredCount = words.Count(x => x.Interval >= 21); // 21天以上视为掌握

        return Ok(new UserWordStatsDto
        {
            TotalWords = words.Count,
            DueCount = dueCount,
            MasteredCount = masteredCount,
            ReviewLogsCount = await _context.WordReviewLogs.CountAsync(x => x.UserId == userId)
        });
    }
}

public class UserWordDto
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public DateTime? NextReview { get; set; }
    public int Interval { get; set; }
    public int RepetitionCount { get; set; }
}

public class AddUserWordRequest
{
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
}

public class ReviewWordRequest
{
    /// <summary>
    /// 评分 0-5: 0=完全忘记, 3=模糊记得, 5=轻松记住
    /// </summary>
    public int Quality { get; set; }
}

public class ReviewResultDto
{
    public DateTime? NextReview { get; set; }
    public int Interval { get; set; }
    public int RepetitionCount { get; set; }
    public double EaseFactor { get; set; }
}

public class UserWordStatsDto
{
    public int TotalWords { get; set; }
    public int DueCount { get; set; }
    public int MasteredCount { get; set; }
    public int ReviewLogsCount { get; set; }
}
