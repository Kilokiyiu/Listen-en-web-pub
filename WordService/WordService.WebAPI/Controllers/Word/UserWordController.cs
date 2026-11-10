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
    /// 获取用户的单词列表（可按单词本筛选）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageResult<UserWordDto>>> GetUserWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] Guid? wordBookId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? reviewMark = null,
        [FromQuery] bool favoritesOnly = false)
    {
        IQueryable<UserWord> query;
        if (favoritesOnly)
        {
            query = _context.UserWords.Where(x => x.UserId == userId && x.IsFavorite);
        }
        else
        {
            var book = await WordBookHelper.ResolveBookAsync(_context, userId, wordBookId);
            if (book == null)
                return NotFound("单词本不存在");

            query = _context.UserWords
                .Where(x => x.UserId == userId && x.WordBookId == book.Id);
        }

        if (!string.IsNullOrEmpty(search))
            query = query.Where(x => x.Word.Contains(search));

        query = ApplyReviewMarkFilter(query, reviewMark);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                WordBookId = x.WordBookId,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount,
                LastQuality = x.LastQuality,
                IsFavorite = x.IsFavorite
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
    /// reviewMark: forget|fuzzy|remember|unreviewed|favorite
    /// </summary>
    private static IQueryable<UserWord> ApplyReviewMarkFilter(IQueryable<UserWord> query, string? reviewMark)
    {
        if (string.IsNullOrWhiteSpace(reviewMark)) return query;
        return reviewMark.Trim().ToLowerInvariant() switch
        {
            "forget" => query.Where(x => x.LastQuality == 0),
            "fuzzy" => query.Where(x => x.LastQuality == 3),
            "remember" => query.Where(x => x.LastQuality == 5),
            "unreviewed" => query.Where(x => x.LastQuality == null),
            "favorite" => query.Where(x => x.IsFavorite),
            _ => query
        };
    }

    /// <summary>
    /// 添加单词到指定单词本（未指定则加入默认本）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserWordDto>> AddUserWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] AddUserWordRequest request)
    {
        var book = await WordBookHelper.ResolveBookAsync(_context, userId, request.WordBookId);
        if (book == null)
            return NotFound("单词本不存在");

        var exists = await _context.UserWords
            .AnyAsync(x => x.UserId == userId
                           && x.WordBookId == book.Id
                           && x.Word.ToLower() == request.Word.ToLower());

        if (exists)
            return BadRequest("该单词本中已存在此单词");

        var word = new UserWord(userId, book.Id, request.Word, request.Definition, request.Example);
        _context.UserWords.Add(word);
        await _context.SaveChangesAsync();

        return Ok(ToDto(word));
    }

    /// <summary>
    /// 批量导入单词到指定单词本（合并去重，不覆盖已有）
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<ImportUserWordsResultDto>> ImportUserWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] ImportUserWordsRequest request,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        if (request.Entries == null || request.Entries.Count == 0)
            return BadRequest("entries 不能为空");

        if (request.Entries.Count > UserWordImportHelper.MaxEntries)
            return BadRequest($"单次最多导入 {UserWordImportHelper.MaxEntries} 条");

        var book = await WordBookHelper.ResolveBookAsync(_context, userId, request.WordBookId);
        if (book == null)
            return NotFound("单词本不存在");

        var result = await UserWordImportHelper.ImportAsync(
            _context, userId, book.Id, request.Entries, ct);

        return Ok(result);
    }

    /// <summary>
    /// 用 AI（DeepSeek）整理杂乱文本为可导入词条（不直接入库；每用户每天限 1 次）
    /// </summary>
    [HttpPost("ai-organize")]
    public async Task<ActionResult<AiOrganizeUserWordsResultDto>> AiOrganizeUserWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] AiOrganizeUserWordsRequest request,
        [FromServices] WordService.WebAPI.Services.IDeepSeekWordImportService aiImport,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        if (!aiImport.IsConfigured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "AI 整理暂未开通");

        var text = request?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("text 不能为空");

        var today = AiOrganizeQuotaHelper.TodayChina();
        var usage = await _context.UserAiOrganizeUsages
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (usage != null && usage.UsedOn(today))
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                "今日 AI 整理次数已用完（每天限 1 次），明天再来吧");
        }

        try
        {
            var entries = await aiImport.OrganizeAsync(text, ct);
            var now = DateTime.Now;
            if (usage == null)
            {
                _context.UserAiOrganizeUsages.Add(
                    new WordService.Domain.Entity.UserAiOrganizeUsage(userId, today, now));
            }
            else
            {
                usage.MarkUsed(today, now);
            }

            await _context.SaveChangesAsync(ct);

            return Ok(new AiOrganizeUserWordsResultDto
            {
                Count = entries.Count,
                RemainingToday = 0,
                Entries = entries.Select(e => new ImportUserWordItem
                {
                    Word = e.Word,
                    Definition = e.Definition,
                    Example = e.Example,
                }).ToList(),
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
        }
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
    /// 批量删除单词
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task<ActionResult<BatchDeleteUserWordsResultDto>> BatchDeleteUserWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] BatchDeleteUserWordsRequest request)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        if (request?.Ids == null || request.Ids.Count == 0)
            return BadRequest("ids 不能为空");

        const int maxIds = 500;
        if (request.Ids.Count > maxIds)
            return BadRequest($"单次最多删除 {maxIds} 条");

        var idSet = request.Ids.Distinct().ToList();
        var words = await _context.UserWords
            .Where(x => x.UserId == userId && idSet.Contains(x.Id))
            .ToListAsync();

        if (words.Count > 0)
        {
            _context.UserWords.RemoveRange(words);
            await _context.SaveChangesAsync();
        }

        return Ok(new BatchDeleteUserWordsResultDto { Deleted = words.Count });
    }

    /// <summary>
    /// 将单词移动到另一个单词本
    /// </summary>
    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult> MoveUserWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id,
        [FromBody] MoveUserWordRequest request)
    {
        var word = await _context.UserWords
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (word == null)
            return NotFound();

        var target = await WordBookHelper.ResolveBookAsync(_context, userId, request.WordBookId);
        if (target == null)
            return NotFound("目标单词本不存在");

        if (word.WordBookId == target.Id)
            return Ok();

        var exists = await _context.UserWords
            .AnyAsync(x => x.UserId == userId
                           && x.WordBookId == target.Id
                           && x.Word.ToLower() == word.Word.ToLower()
                           && x.Id != word.Id);
        if (exists)
            return BadRequest("目标单词本中已存在同名单词");

        word.MoveToBook(target.Id);
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 获取待复习的单词
    /// </summary>
    [HttpGet("due")]
    public async Task<ActionResult<List<UserWordDto>>> GetDueWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] Guid? wordBookId = null,
        [FromQuery] int limit = 20,
        [FromQuery] bool favoritesOnly = false)
    {
        IQueryable<UserWord> query;
        if (favoritesOnly)
        {
            query = _context.UserWords.Where(x => x.UserId == userId && x.IsFavorite);
        }
        else
        {
            var book = await WordBookHelper.ResolveBookAsync(_context, userId, wordBookId);
            if (book == null)
                return NotFound("单词本不存在");
            query = _context.UserWords.Where(x => x.UserId == userId && x.WordBookId == book.Id);
        }

        var now = DateTime.Now;
        var words = await query
            .Where(x => x.NextReview == null || x.NextReview <= now)
            .OrderBy(x => x.NextReview)
            .Take(limit)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                WordBookId = x.WordBookId,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount,
                LastQuality = x.LastQuality,
                IsFavorite = x.IsFavorite
            })
            .ToListAsync();

        return Ok(words);
    }

    /// <summary>
    /// 自由复习：随机返回用户当前单词本中未掌握的单词
    /// </summary>
    [HttpGet("random")]
    public async Task<ActionResult<List<UserWordDto>>> GetRandomWords(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] Guid? wordBookId = null,
        [FromQuery] int limit = 50,
        [FromQuery] bool favoritesOnly = false)
    {
        IQueryable<UserWord> query;
        if (favoritesOnly)
        {
            query = _context.UserWords.Where(x => x.UserId == userId && x.IsFavorite);
        }
        else
        {
            var book = await WordBookHelper.ResolveBookAsync(_context, userId, wordBookId);
            if (book == null)
                return NotFound("单词本不存在");
            query = _context.UserWords.Where(x => x.UserId == userId && x.WordBookId == book.Id);
        }

        var words = await query
            .Where(x => x.Interval < 21)
            .OrderBy(x => Guid.NewGuid())
            .Take(limit)
            .Select(x => new UserWordDto
            {
                Id = x.Id,
                WordBookId = x.WordBookId,
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
                NextReview = x.NextReview,
                Interval = x.Interval,
                RepetitionCount = x.RepetitionCount,
                LastQuality = x.LastQuality,
                IsFavorite = x.IsFavorite
            })
            .ToListAsync();

        return Ok(words);
    }

    /// <summary>
    /// 切换单词收藏
    /// </summary>
    [HttpPost("{id:guid}/favorite")]
    public async Task<ActionResult<object>> ToggleFavoriteWord(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id,
        [FromBody] ToggleFavoriteRequest? request = null)
    {
        var word = await _context.UserWords
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (word == null)
            return NotFound();

        if (request?.IsFavorite is bool preferred)
            word.SetFavorite(preferred);
        else
            word.ToggleFavorite();

        await _context.SaveChangesAsync();
        return Ok(new { id = word.Id, isFavorite = word.IsFavorite });
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

        word.UpdateReview(request.Quality);

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
    /// 获取用户学习统计（可按单词本筛选）
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<UserWordStatsDto>> GetStats(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromQuery] Guid? wordBookId = null,
        [FromQuery] bool favoritesOnly = false)
    {
        List<UserWord> words;
        Guid? bookId = null;
        string bookName;

        if (favoritesOnly)
        {
            words = await _context.UserWords
                .Where(x => x.UserId == userId && x.IsFavorite)
                .ToListAsync();
            bookName = "收藏";
        }
        else
        {
            var book = await WordBookHelper.ResolveBookAsync(_context, userId, wordBookId);
            if (book == null)
                return NotFound("单词本不存在");

            words = await _context.UserWords
                .Where(x => x.UserId == userId && x.WordBookId == book.Id)
                .ToListAsync();
            bookId = book.Id;
            bookName = book.Name;
        }

        var now = DateTime.Now;
        var dueCount = words.Count(x => x.NextReview == null || x.NextReview <= now);
        var masteredCount = words.Count(x => x.Interval >= 21);
        var wordIds = words.Select(x => x.Id).ToList();

        return Ok(new UserWordStatsDto
        {
            TotalWords = words.Count,
            DueCount = dueCount,
            MasteredCount = masteredCount,
            ReviewLogsCount = await _context.WordReviewLogs
                .CountAsync(x => x.UserId == userId && wordIds.Contains(x.WordId)),
            WordBookId = bookId,
            WordBookName = bookName
        });
    }

    private static UserWordDto ToDto(UserWord x) => new()
    {
        Id = x.Id,
        WordBookId = x.WordBookId,
        Word = x.Word,
        Definition = x.Definition,
        Example = x.Example,
        NextReview = x.NextReview,
        Interval = x.Interval,
        RepetitionCount = x.RepetitionCount,
        LastQuality = x.LastQuality,
        IsFavorite = x.IsFavorite
    };
}

public class UserWordDto
{
    public Guid Id { get; set; }
    public Guid WordBookId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public DateTime? NextReview { get; set; }
    public int Interval { get; set; }
    public int RepetitionCount { get; set; }
    public int? LastQuality { get; set; }
    public bool IsFavorite { get; set; }
}

public class ToggleFavoriteRequest
{
    public bool? IsFavorite { get; set; }
}

public class AddUserWordRequest
{
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public Guid? WordBookId { get; set; }
}

public class BatchDeleteUserWordsRequest
{
    public List<Guid> Ids { get; set; } = new();
}

public class BatchDeleteUserWordsResultDto
{
    public int Deleted { get; set; }
}

public class ImportUserWordsRequest
{
    public Guid? WordBookId { get; set; }
    public List<ImportUserWordItem> Entries { get; set; } = new();
}

public class AiOrganizeUserWordsRequest
{
    public string Text { get; set; } = string.Empty;
}

public class AiOrganizeUserWordsResultDto
{
    public int Count { get; set; }
    /// <summary>今日剩余可用次数（成功后一般为 0）</summary>
    public int RemainingToday { get; set; }
    public List<ImportUserWordItem> Entries { get; set; } = new();
}

internal static class AiOrganizeQuotaHelper
{
    public static DateOnly TodayChina()
    {
        var nowUtc = DateTime.UtcNow;
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }

        var local = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
        return DateOnly.FromDateTime(local);
    }
}

public class ImportUserWordItem
{
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
}

public class ImportUserWordsResultDto
{
    public Guid WordBookId { get; set; }
    public int Total { get; set; }
    public int Added { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
}

public static class UserWordImportHelper
{
    public const int MaxEntries = 5000;

    public static async Task<ImportUserWordsResultDto> ImportAsync(
        WordDbContext context,
        Guid userId,
        Guid wordBookId,
        IEnumerable<ImportUserWordItem> entries,
        CancellationToken ct = default)
    {
        var existing = await context.UserWords
            .Where(x => x.UserId == userId && x.WordBookId == wordBookId)
            .Select(x => x.Word)
            .ToListAsync(ct);
        var keys = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var skipped = 0;
        var failed = 0;
        var total = 0;

        foreach (var item in entries)
        {
            total++;
            var wordText = (item.Word ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(wordText) || wordText.Length > 200)
            {
                failed++;
                continue;
            }

            if (keys.Contains(wordText))
            {
                skipped++;
                continue;
            }

            var definition = TrimOrNull(item.Definition, 1000);
            var example = TrimOrNull(item.Example, 2000);
            context.UserWords.Add(new UserWord(userId, wordBookId, wordText, definition, example));
            keys.Add(wordText);
            added++;
        }

        if (added > 0)
            await context.SaveChangesAsync(ct);

        return new ImportUserWordsResultDto
        {
            WordBookId = wordBookId,
            Total = total,
            Added = added,
            Skipped = skipped,
            Failed = failed
        };
    }

    private static string? TrimOrNull(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var t = value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }
}

public class MoveUserWordRequest
{
    public Guid WordBookId { get; set; }
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
    public Guid? WordBookId { get; set; }
    public string? WordBookName { get; set; }
}
