using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Admin;

[ApiController]
[Route("Admin/WordPacks")]
[Authorize(Roles = "Admin")]
public class AdminWordPackController : ControllerBase
{
    private readonly WordDbContext _db;

    public AdminWordPackController(WordDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var packs = await _db.WordPacks
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreationTime)
            .Select(x => new WordPackAdminDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Category = x.Category,
                IsPublished = x.IsPublished,
                SortOrder = x.SortOrder,
                CreationTime = x.CreationTime,
                UpdatedAt = x.UpdatedAt,
                WordCount = _db.WordPackEntries.Count(e => e.WordPackId == x.Id),
            })
            .ToListAsync(ct);

        return Ok(new { code = 200, data = packs });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertWordPackRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("code 与 name 不能为空");

        var code = WordPack.NormalizeCode(request.Code);
        if (await _db.WordPacks.AnyAsync(x => x.Code == code, ct))
            return BadRequest("词本编码已存在");

        var pack = new WordPack(code, request.Name, request.Category ?? "other", request.Description, request.SortOrder);
        if (request.IsPublished == true)
            pack.SetPublished(true);

        _db.WordPacks.Add(pack);
        await _db.SaveChangesAsync(ct);

        return Ok(new { code = 200, data = ToAdminDto(pack, 0) });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertWordPackRequest request, CancellationToken ct)
    {
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pack == null)
            return NotFound("词本不存在");

        pack.UpdateMeta(request.Name ?? pack.Name, request.Category ?? pack.Category, request.Description, request.SortOrder);
        if (request.IsPublished.HasValue)
            pack.SetPublished(request.IsPublished.Value);

        await _db.SaveChangesAsync(ct);
        var count = await _db.WordPackEntries.CountAsync(x => x.WordPackId == id, ct);
        return Ok(new { code = 200, data = ToAdminDto(pack, count) });
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> TogglePublish(Guid id, CancellationToken ct)
    {
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pack == null)
            return NotFound("词本不存在");

        pack.SetPublished(!pack.IsPublished);
        await _db.SaveChangesAsync(ct);
        return Ok(new { code = 200, data = new { pack.Id, pack.IsPublished } });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pack == null)
            return NotFound("词本不存在");

        _db.WordPacks.Remove(pack);
        await _db.SaveChangesAsync(ct);
        return Ok(new { code = 200, message = "已删除" });
    }

    [HttpGet("{id:guid}/entries")]
    public async Task<IActionResult> ListEntries(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        if (!await _db.WordPacks.AnyAsync(x => x.Id == id, ct))
            return NotFound("词本不存在");

        var query = _db.WordPackEntries.Where(x => x.WordPackId == id);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => x.Word.Contains(s) || (x.Definition != null && x.Definition.Contains(s)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WordPackEntryDto
            {
                Id = x.Id,
                Word = x.Word,
                Phonetic = x.Phonetic,
                Definition = x.Definition,
                Example = x.Example,
                Rank = x.Rank,
                CreationTime = x.CreationTime,
            })
            .ToListAsync(ct);

        return Ok(new { code = 200, data = new { items, total, page, pageSize } });
    }

    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> AddEntry(Guid id, [FromBody] UpsertWordPackEntryRequest request, CancellationToken ct)
    {
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pack == null)
            return NotFound("词本不存在");

        if (string.IsNullOrWhiteSpace(request.Word))
            return BadRequest("单词不能为空");

        var word = WordPackEntry.NormalizeWord(request.Word);
        var exists = await _db.WordPackEntries.AnyAsync(
            x => x.WordPackId == id && x.Word.ToLower() == word.ToLower(), ct);
        if (exists)
            return BadRequest("该词本中已存在此单词");

        var entry = new WordPackEntry(id, word, request.Definition, request.Example, request.Phonetic, request.Rank);
        _db.WordPackEntries.Add(entry);
        pack.Touch();
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            code = 200,
            data = new WordPackEntryDto
            {
                Id = entry.Id,
                Word = entry.Word,
                Phonetic = entry.Phonetic,
                Definition = entry.Definition,
                Example = entry.Example,
                Rank = entry.Rank,
                CreationTime = entry.CreationTime,
            }
        });
    }

    [HttpPut("{packId:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> UpdateEntry(
        Guid packId,
        Guid entryId,
        [FromBody] UpsertWordPackEntryRequest request,
        CancellationToken ct)
    {
        var entry = await _db.WordPackEntries
            .FirstOrDefaultAsync(x => x.Id == entryId && x.WordPackId == packId, ct);
        if (entry == null)
            return NotFound("词条不存在");

        entry.Update(request.Definition, request.Example, request.Phonetic, request.Rank);
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == packId, ct);
        pack?.Touch();
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            code = 200,
            data = new WordPackEntryDto
            {
                Id = entry.Id,
                Word = entry.Word,
                Phonetic = entry.Phonetic,
                Definition = entry.Definition,
                Example = entry.Example,
                Rank = entry.Rank,
                CreationTime = entry.CreationTime,
            }
        });
    }

    [HttpDelete("{packId:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid packId, Guid entryId, CancellationToken ct)
    {
        var entry = await _db.WordPackEntries
            .FirstOrDefaultAsync(x => x.Id == entryId && x.WordPackId == packId, ct);
        if (entry == null)
            return NotFound("词条不存在");

        _db.WordPackEntries.Remove(entry);
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == packId, ct);
        pack?.Touch();
        await _db.SaveChangesAsync(ct);
        return Ok(new { code = 200, message = "已删除" });
    }

    /// <summary>
    /// 批量导入词条。默认跳过已存在单词；replaceExisting=true 时覆盖释义。
    /// </summary>
    [HttpPost("{id:guid}/entries/import")]
    public async Task<IActionResult> ImportEntries(
        Guid id,
        [FromBody] ImportWordPackEntriesRequest request,
        CancellationToken ct)
    {
        var pack = await _db.WordPacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pack == null)
            return NotFound("词本不存在");

        if (request.Entries == null || request.Entries.Count == 0)
            return BadRequest("entries 不能为空");

        if (request.Entries.Count > 8000)
            return BadRequest("单次最多导入 8000 条");

        var existing = await _db.WordPackEntries
            .Where(x => x.WordPackId == id)
            .ToListAsync(ct);
        var byWord = existing.ToDictionary(x => x.Word.ToLowerInvariant(), x => x);

        var added = 0;
        var updated = 0;
        var skipped = 0;
        var rankFallback = existing.Count == 0 ? 0 : existing.Max(x => x.Rank);

        foreach (var item in request.Entries)
        {
            if (string.IsNullOrWhiteSpace(item.Word))
            {
                skipped++;
                continue;
            }

            var word = WordPackEntry.NormalizeWord(item.Word);
            var key = word.ToLowerInvariant();
            if (byWord.TryGetValue(key, out var existingEntry))
            {
                if (request.ReplaceExisting)
                {
                    existingEntry.Update(item.Definition, item.Example, item.Phonetic, item.Rank);
                    updated++;
                }
                else
                {
                    skipped++;
                }
                continue;
            }

            var rank = item.Rank > 0 ? item.Rank : ++rankFallback;
            var entry = new WordPackEntry(id, word, item.Definition, item.Example, item.Phonetic, rank);
            _db.WordPackEntries.Add(entry);
            byWord[key] = entry;
            added++;
        }

        pack.Touch();
        await _db.SaveChangesAsync(ct);

        var total = await _db.WordPackEntries.CountAsync(x => x.WordPackId == id, ct);
        return Ok(new
        {
            code = 200,
            data = new { added, updated, skipped, total }
        });
    }

    private static WordPackAdminDto ToAdminDto(WordPack pack, int wordCount) => new()
    {
        Id = pack.Id,
        Code = pack.Code,
        Name = pack.Name,
        Description = pack.Description,
        Category = pack.Category,
        IsPublished = pack.IsPublished,
        SortOrder = pack.SortOrder,
        CreationTime = pack.CreationTime,
        UpdatedAt = pack.UpdatedAt,
        WordCount = wordCount,
    };
}

public class WordPackAdminDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int WordCount { get; set; }
}

public class WordPackEntryDto
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public int Rank { get; set; }
    public DateTime CreationTime { get; set; }
}

public class UpsertWordPackRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public bool? IsPublished { get; set; }
}

public class UpsertWordPackEntryRequest
{
    public string? Word { get; set; }
    public string? Phonetic { get; set; }
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public int Rank { get; set; }
}

public class ImportWordPackEntriesRequest
{
    public List<ImportWordPackEntryItem> Entries { get; set; } = new();
    public bool ReplaceExisting { get; set; }
}

public class ImportWordPackEntryItem
{
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public int Rank { get; set; }
}
