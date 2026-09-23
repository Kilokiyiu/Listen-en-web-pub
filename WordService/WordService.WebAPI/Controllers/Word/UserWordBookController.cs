using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("word-books")]
public class UserWordBookController : ControllerBase
{
    private readonly WordDbContext _context;

    public UserWordBookController(WordDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取当前用户的全部单词本（无则自动创建默认单词本）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserWordBookDto>>> GetWordBooks(
        [FromHeader(Name = "X-User-Id")] Guid userId)
    {
        await WordBookHelper.EnsureDefaultBookAsync(_context, userId);

        var books = await _context.UserWordBooks
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreationTime)
            .Select(x => new UserWordBookDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsDefault = x.IsDefault,
                SortOrder = x.SortOrder,
                CreationTime = x.CreationTime,
                WordCount = _context.UserWords.Count(w => w.WordBookId == x.Id)
            })
            .ToListAsync();

        return Ok(books);
    }

    /// <summary>
    /// 新建单词本
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserWordBookDto>> CreateWordBook(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] CreateWordBookRequest request)
    {
        await WordBookHelper.EnsureDefaultBookAsync(_context, userId);

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("单词本名称不能为空");

        var name = request.Name.Trim();
        if (name.Length > 100)
            return BadRequest("单词本名称过长");

        var exists = await _context.UserWordBooks
            .AnyAsync(x => x.UserId == userId && x.Name == name);
        if (exists)
            return BadRequest("已存在同名单词本");

        var book = new UserWordBook(userId, name, isDefault: false, request.Description);
        _context.UserWordBooks.Add(book);
        await _context.SaveChangesAsync();

        return Ok(new UserWordBookDto
        {
            Id = book.Id,
            Name = book.Name,
            Description = book.Description,
            IsDefault = book.IsDefault,
            SortOrder = book.SortOrder,
            CreationTime = book.CreationTime,
            WordCount = 0
        });
    }

    /// <summary>
    /// 重命名 / 更新描述
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserWordBookDto>> UpdateWordBook(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id,
        [FromBody] UpdateWordBookRequest request)
    {
        var book = await _context.UserWordBooks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (book == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            var conflict = await _context.UserWordBooks
                .AnyAsync(x => x.UserId == userId && x.Name == name && x.Id != id);
            if (conflict)
                return BadRequest("已存在同名单词本");
            book.Rename(name);
        }

        if (request.Description != null)
            book.UpdateDescription(request.Description);

        await _context.SaveChangesAsync();

        return Ok(new UserWordBookDto
        {
            Id = book.Id,
            Name = book.Name,
            Description = book.Description,
            IsDefault = book.IsDefault,
            SortOrder = book.SortOrder,
            CreationTime = book.CreationTime,
            WordCount = await _context.UserWords.CountAsync(w => w.WordBookId == book.Id)
        });
    }

    /// <summary>
    /// 删除单词本（默认本不可删；有单词时需确认迁移到默认本）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteWordBook(
        [FromHeader(Name = "X-User-Id")] Guid userId,
        Guid id,
        [FromQuery] bool moveWordsToDefault = true)
    {
        var book = await _context.UserWordBooks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (book == null)
            return NotFound();

        if (book.IsDefault)
            return BadRequest("默认单词本不能删除");

        var defaultBook = await WordBookHelper.EnsureDefaultBookAsync(_context, userId);
        if (defaultBook.Id == book.Id)
            return BadRequest("默认单词本不能删除");

        var words = await _context.UserWords
            .Where(x => x.WordBookId == book.Id)
            .ToListAsync();

        if (words.Count > 0)
        {
            if (!moveWordsToDefault)
                return BadRequest("单词本中还有单词，请先清空或选择迁移到默认本");

            foreach (var word in words)
                word.MoveToBook(defaultBook.Id);
        }

        _context.UserWordBooks.Remove(book);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public static class WordBookHelper
{
    public const string DefaultBookName = "默认单词本";

    public static async Task<UserWordBook> EnsureDefaultBookAsync(WordDbContext context, Guid userId)
    {
        var book = await context.UserWordBooks
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault);

        if (book != null)
            return book;

        book = await context.UserWordBooks
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreationTime)
            .FirstOrDefaultAsync();

        if (book != null)
        {
            book.SetDefault(true);
            await context.SaveChangesAsync();
            return book;
        }

        book = new UserWordBook(userId, DefaultBookName, isDefault: true);
        context.UserWordBooks.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public static async Task<UserWordBook?> ResolveBookAsync(
        WordDbContext context,
        Guid userId,
        Guid? wordBookId)
    {
        var defaultBook = await EnsureDefaultBookAsync(context, userId);
        if (wordBookId == null || wordBookId == Guid.Empty)
            return defaultBook;

        return await context.UserWordBooks
            .FirstOrDefaultAsync(x => x.Id == wordBookId && x.UserId == userId);
    }
}

public class UserWordBookDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreationTime { get; set; }
    public int WordCount { get; set; }
}

public class CreateWordBookRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateWordBookRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
