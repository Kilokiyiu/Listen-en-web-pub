using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("word-packs")]
public class WordPackController : ControllerBase
{
    private readonly WordDbContext _db;

    public WordPackController(WordDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 已发布官方词本列表（可匿名浏览）。若带 X-User-Id，附带领取状态。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken ct)
    {
        var packs = await _db.WordPacks
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Category,
                x.SortOrder,
                WordCount = _db.WordPackEntries.Count(e => e.WordPackId == x.Id),
            })
            .ToListAsync(ct);

        Dictionary<Guid, UserWordPackClaim>? claims = null;
        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            claims = await _db.UserWordPackClaims
                .Where(x => x.UserId == userId.Value)
                .ToDictionaryAsync(x => x.WordPackId, ct);
        }

        var data = packs.Select(p =>
        {
            UserWordPackClaim? claim = null;
            if (claims != null)
                claims.TryGetValue(p.Id, out claim);

            return new
            {
                p.Id,
                p.Code,
                p.Name,
                p.Description,
                p.Category,
                p.SortOrder,
                p.WordCount,
                Claimed = claim != null,
                ClaimedCount = claim?.ClaimedCount ?? 0,
                UserWordBookId = claim?.UserWordBookId,
            };
        });

        return Ok(data);
    }

    /// <summary>
    /// 词本预览（前 N 条），已发布才可看。
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(
        Guid id,
        [FromQuery] int preview = 20,
        [FromHeader(Name = "X-User-Id")] Guid? userId = null,
        CancellationToken ct = default)
    {
        preview = Math.Clamp(preview, 0, 50);
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);
        if (pack == null)
            return NotFound("词本不存在或未发布");

        var wordCount = await _db.WordPackEntries.CountAsync(x => x.WordPackId == id, ct);
        var entries = await _db.WordPackEntries
            .Where(x => x.WordPackId == id)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Word)
            .Take(preview)
            .Select(x => new
            {
                x.Word,
                x.Phonetic,
                x.Definition,
                x.Example,
                x.Rank,
            })
            .ToListAsync(ct);

        UserWordPackClaim? claim = null;
        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            claim = await _db.UserWordPackClaims
                .FirstOrDefaultAsync(x => x.UserId == userId.Value && x.WordPackId == id, ct);
        }

        return Ok(new
        {
            pack.Id,
            pack.Code,
            pack.Name,
            pack.Description,
            pack.Category,
            WordCount = wordCount,
            Preview = entries,
            Claimed = claim != null,
            ClaimedCount = claim?.ClaimedCount ?? 0,
            UserWordBookId = claim?.UserWordBookId,
        });
    }

    /// <summary>
    /// 领取官方词本：创建/复用个人单词本，并复制尚未拥有的词条。
    /// </summary>
    [HttpPost("{id:guid}/claim")]
    public async Task<IActionResult> Claim(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] ClaimWordPackRequest? request,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished, ct);
        if (pack == null)
            return NotFound("词本不存在或未发布");

        var limit = request?.Limit is > 0 and <= 5000 ? request.Limit.Value : 5000;

        var claim = await _db.UserWordPackClaims
            .FirstOrDefaultAsync(x => x.UserId == userId && x.WordPackId == id, ct);

        UserWordBook book;
        if (claim != null)
        {
            book = await _db.UserWordBooks
                .FirstOrDefaultAsync(x => x.Id == claim.UserWordBookId && x.UserId == userId, ct)
                ?? await CreateClaimBookAsync(userId, pack, ct);
            if (claim.UserWordBookId != book.Id)
            {
                // 原词本被删，挂到新本
                _db.UserWordPackClaims.Remove(claim);
                claim = null;
            }
        }
        else
        {
            book = await FindOrCreateClaimBookAsync(userId, pack, ct);
        }

        var existingWords = await _db.UserWords
            .Where(x => x.UserId == userId && x.WordBookId == book.Id)
            .Select(x => x.Word.ToLower())
            .ToListAsync(ct);
        var existingSet = existingWords.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidates = await _db.WordPackEntries
            .Where(x => x.WordPackId == id)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Word)
            .ToListAsync(ct);

        var toAdd = candidates
            .Where(x => !existingSet.Contains(x.Word))
            .Take(limit)
            .ToList();

        foreach (var entry in toAdd)
        {
            var definition = entry.Definition;
            if (!string.IsNullOrWhiteSpace(entry.Phonetic))
            {
                definition = string.IsNullOrWhiteSpace(definition)
                    ? entry.Phonetic
                    : $"{entry.Phonetic} {definition}";
            }

            _db.UserWords.Add(new UserWord(userId, book.Id, entry.Word, definition, entry.Example));
        }

        var added = toAdd.Count;
        var totalInBook = existingSet.Count + added;

        if (claim == null)
        {
            claim = new UserWordPackClaim(userId, id, book.Id, totalInBook);
            _db.UserWordPackClaims.Add(claim);
        }
        else
        {
            claim.UpdateProgress(totalInBook);
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            wordPackId = id,
            userWordBookId = book.Id,
            userWordBookName = book.Name,
            added,
            totalInBook,
            packWordCount = candidates.Count,
            remaining = Math.Max(0, candidates.Count - totalInBook),
        });
    }

    private async Task<UserWordBook> FindOrCreateClaimBookAsync(Guid userId, WordPack pack, CancellationToken ct)
    {
        await WordBookHelper.EnsureDefaultBookAsync(_db, userId);

        var preferredName = pack.Name.Trim();
        var existing = await _db.UserWordBooks
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == preferredName, ct);
        if (existing != null)
            return existing;

        return await CreateClaimBookAsync(userId, pack, ct);
    }

    private async Task<UserWordBook> CreateClaimBookAsync(Guid userId, WordPack pack, CancellationToken ct)
    {
        var name = pack.Name.Trim();
        var baseName = name;
        var suffix = 1;
        while (await _db.UserWordBooks.AnyAsync(x => x.UserId == userId && x.Name == name, ct))
        {
            suffix++;
            name = $"{baseName} ({suffix})";
        }

        var book = new UserWordBook(userId, name, isDefault: false, pack.Description);
        _db.UserWordBooks.Add(book);
        await _db.SaveChangesAsync(ct);
        return book;
    }
}

public class ClaimWordPackRequest
{
    /// <summary>本次最多复制多少未拥有的词，默认 5000。</summary>
    public int? Limit { get; set; }
}
