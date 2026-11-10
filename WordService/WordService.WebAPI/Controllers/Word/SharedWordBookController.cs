using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("shared-wordbooks")]
public class SharedWordBookController : ControllerBase
{
    public const int MaxSnapshotWords = 5000;
    public const int DefaultExpireDays = 30;
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly WordDbContext _db;

    public SharedWordBookController(WordDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 预览分享词本（可匿名）
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetPreview(string code, [FromQuery] int preview = 20, CancellationToken ct = default)
    {
        preview = Math.Clamp(preview, 0, 50);
        var normalized = SharedWordBook.NormalizeCode(code);
        if (string.IsNullOrWhiteSpace(normalized))
            return NotFound("分享码无效");

        var share = await _db.SharedWordBooks
            .FirstOrDefaultAsync(x => x.Code == normalized, ct);
        if (share == null)
            return NotFound("分享不存在");

        var now = DateTime.Now;
        if (!share.IsAvailable(now))
            return BadRequest(share.IsRevoked ? "分享已取消" : "分享已过期");

        var wordCount = await _db.SharedWordBookEntries
            .CountAsync(x => x.SharedWordBookId == share.Id, ct);
        var entries = await _db.SharedWordBookEntries
            .Where(x => x.SharedWordBookId == share.Id)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Word)
            .Take(preview)
            .Select(x => new { x.Word, x.Definition, x.Example })
            .ToListAsync(ct);

        return Ok(new
        {
            share.Code,
            share.Title,
            WordCount = wordCount,
            share.ExpiresAt,
            Preview = entries,
        });
    }

    /// <summary>
    /// 领取分享词本：默认新建「来自 xxx」词本；也可合并到已有词本
    /// </summary>
    [HttpPost("{code}/claim")]
    public async Task<IActionResult> Claim(
        string code,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] ClaimSharedWordBookRequest? request,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        var normalized = SharedWordBook.NormalizeCode(code);
        var share = await _db.SharedWordBooks
            .FirstOrDefaultAsync(x => x.Code == normalized, ct);
        if (share == null)
            return NotFound("分享不存在");

        var now = DateTime.Now;
        if (!share.IsAvailable(now))
            return BadRequest(share.IsRevoked ? "分享已取消" : "分享已过期");

        UserWordBook book;
        if (request?.WordBookId is Guid targetId && targetId != Guid.Empty)
        {
            var resolved = await WordBookHelper.ResolveBookAsync(_db, userId, targetId);
            if (resolved == null)
                return NotFound("目标单词本不存在");
            book = resolved;
        }
        else
        {
            book = await CreateClaimBookAsync(userId, share.Title, ct);
        }

        var entries = await _db.SharedWordBookEntries
            .Where(x => x.SharedWordBookId == share.Id)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Word)
            .Take(MaxSnapshotWords)
            .Select(x => new ImportUserWordItem
            {
                Word = x.Word,
                Definition = x.Definition,
                Example = x.Example,
            })
            .ToListAsync(ct);

        var import = await UserWordImportHelper.ImportAsync(_db, userId, book.Id, entries, ct);

        return Ok(new
        {
            code = share.Code,
            title = share.Title,
            userWordBookId = book.Id,
            userWordBookName = book.Name,
            import.Added,
            import.Skipped,
            import.Failed,
            import.Total,
        });
    }

    /// <summary>
    /// 所有者作废分享码
    /// </summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> Revoke(
        string code,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Unauthorized("请先登录");

        var normalized = SharedWordBook.NormalizeCode(code);
        var share = await _db.SharedWordBooks
            .FirstOrDefaultAsync(x => x.Code == normalized, ct);
        if (share == null)
            return NotFound("分享不存在");
        if (share.OwnerUserId != userId)
            return BadRequest("只能取消自己创建的分享");

        share.Revoke();
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    internal static async Task<(SharedWordBook share, int wordCount)> CreateShareAsync(
        WordDbContext db,
        Guid userId,
        UserWordBook book,
        CancellationToken ct)
    {
        var words = await db.UserWords
            .Where(x => x.UserId == userId && x.WordBookId == book.Id)
            .OrderBy(x => x.CreationTime)
            .Take(MaxSnapshotWords)
            .ToListAsync(ct);

        if (words.Count == 0)
            throw new InvalidOperationException("单词本没有可分享的单词");

        var code = await GenerateUniqueCodeAsync(db, ct);
        var share = new SharedWordBook(
            userId,
            book.Id,
            book.Name,
            code,
            DateTime.Now.AddDays(DefaultExpireDays));

        db.SharedWordBooks.Add(share);
        var rank = 0;
        foreach (var w in words)
        {
            rank++;
            db.SharedWordBookEntries.Add(
                new SharedWordBookEntry(share.Id, w.Word, w.Definition, w.Example, rank));
        }

        await db.SaveChangesAsync(ct);
        return (share, words.Count);
    }

    private async Task<UserWordBook> CreateClaimBookAsync(Guid userId, string title, CancellationToken ct)
    {
        await WordBookHelper.EnsureDefaultBookAsync(_db, userId);

        var preferredName = $"来自 {title}".Trim();
        if (preferredName.Length > 100)
            preferredName = preferredName[..100];

        var name = preferredName;
        var baseName = preferredName;
        var suffix = 1;
        while (await _db.UserWordBooks.AnyAsync(x => x.UserId == userId && x.Name == name, ct))
        {
            suffix++;
            name = $"{baseName} ({suffix})";
            if (name.Length > 100)
                name = name[..100];
        }

        var book = new UserWordBook(userId, name, isDefault: false, description: $"分享领取：{title}");
        _db.UserWordBooks.Add(book);
        await _db.SaveChangesAsync(ct);
        return book;
    }

    private static async Task<string> GenerateUniqueCodeAsync(WordDbContext db, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = GenerateCode(6);
            var exists = await db.SharedWordBooks.AnyAsync(x => x.Code == code, ct);
            if (!exists)
                return code;
        }

        throw new InvalidOperationException("无法生成唯一分享码，请重试");
    }

    private static string GenerateCode(int length)
    {
        var chars = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);
        for (var i = 0; i < length; i++)
            chars[i] = CodeAlphabet[bytes[i] % CodeAlphabet.Length];
        return new string(chars);
    }
}

public class ClaimSharedWordBookRequest
{
    /// <summary>合并到已有词本；为空则新建「来自 xxx」</summary>
    public Guid? WordBookId { get; set; }
}
