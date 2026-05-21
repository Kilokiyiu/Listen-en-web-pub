using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Domain.Entity;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("word-roots")]
public class WordRootController : ControllerBase
{
    private readonly WordDbContext _context;

    public WordRootController(WordDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有词根（分页）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageResult<WordRootDto>>> GetWordRoots(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? origin = null,
        [FromQuery] string? search = null)
    {
        var query = _context.WordRoots.AsQueryable();

        if (!string.IsNullOrEmpty(origin))
            query = query.Where(x => x.Origin == origin);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(x => x.Root.Contains(search) || x.Meaning.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.RootId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WordRootDto
            {
                Id = x.Id,
                RootId = x.RootId,
                Root = x.Root,
                Origin = x.Origin,
                Meaning = x.Meaning,
                MeaningEn = x.MeaningEn,
                Description = x.Description,
                ExampleCount = x.Examples.Count
            })
            .ToListAsync();

        return Ok(new PageResult<WordRootDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// 获取词根详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WordRootDetailDto>> GetWordRoot(Guid id)
    {
        var wordRoot = await _context.WordRoots
            .Include(x => x.Examples)
            .Include(x => x.Quizzes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (wordRoot == null)
            return NotFound();

        return Ok(new WordRootDetailDto
        {
            Id = wordRoot.Id,
            RootId = wordRoot.RootId,
            Root = wordRoot.Root,
            Origin = wordRoot.Origin,
            Meaning = wordRoot.Meaning,
            MeaningEn = wordRoot.MeaningEn,
            Description = wordRoot.Description,
            Examples = wordRoot.Examples.Select(e => new WordRootExampleDto
            {
                Id = e.Id,
                Word = e.Word,
                Prefix = e.Prefix,
                Root = e.Root,
                Suffix = e.Suffix,
                Meaning = e.Meaning,
                Explanation = e.Explanation
            }).ToList(),
            Quizzes = wordRoot.Quizzes.Select(q => new WordRootQuizDto
            {
                Id = q.Id,
                Question = q.Question,
                Options = q.GetOptions().ToList(),
                CorrectAnswer = q.CorrectAnswer
            }).ToList()
        });
    }

    /// <summary>
    /// 获取测验题
    /// </summary>
    [HttpGet("{id:guid}/quiz")]
    public async Task<ActionResult<WordRootQuizDto>> GetQuiz(Guid id)
    {
        var quiz = await _context.WordRootQuizzes
            .Where(q => q.WordRootId == id)
            .Select(q => new WordRootQuizDto
            {
                Id = q.Id,
                Question = q.Question,
                Options = q.GetOptions().ToList(),
                CorrectAnswer = q.CorrectAnswer
            })
            .FirstOrDefaultAsync();

        if (quiz == null)
            return NotFound();

        return Ok(quiz);
    }
}

// DTOs
public class WordRootDto
{
    public Guid Id { get; set; }
    public int RootId { get; set; }
    public string Root { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? MeaningEn { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ExampleCount { get; set; }
}

public class WordRootDetailDto : WordRootDto
{
    public new List<WordRootExampleDto> Examples { get; set; } = new();
    public new List<WordRootQuizDto> Quizzes { get; set; } = new();
}

public class WordRootExampleDto
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string? Root { get; set; }
    public string? Suffix { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}

public class WordRootQuizDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
}

public class PageResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
