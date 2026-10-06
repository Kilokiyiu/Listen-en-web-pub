using DomainCommons;

namespace WordService.Domain.Entity;

/// <summary>
/// 官方词本（四级 / 六级 / 考研等），用户领取后复制到个人单词本复习。
/// </summary>
public class WordPack : ICreationTime
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>cet4 / cet6 / kaoyan / other</summary>
    public string Category { get; private set; } = "other";
    public bool IsPublished { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<WordPackEntry> _entries = new();
    public IReadOnlyCollection<WordPackEntry> Entries => _entries;

    private WordPack() { }

    public WordPack(string code, string name, string category, string? description = null, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        Code = NormalizeCode(code);
        Name = name.Trim();
        Category = NormalizeCategory(category);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
        IsPublished = false;
        CreationTime = DateTime.Now;
        UpdatedAt = CreationTime;
    }

    public void UpdateMeta(string name, string category, string? description, int sortOrder)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();
        Category = NormalizeCategory(category);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
        Touch();
    }

    public void SetPublished(bool published)
    {
        IsPublished = published;
        Touch();
    }

    public void Touch() => UpdatedAt = DateTime.Now;

    public static string NormalizeCode(string code) =>
        (code ?? string.Empty).Trim().ToLowerInvariant();

    public static string NormalizeCategory(string category)
    {
        var c = (category ?? "other").Trim().ToLowerInvariant();
        return c switch
        {
            "cet4" or "cet-4" or "四级" => "cet4",
            "cet6" or "cet-6" or "六级" => "cet6",
            "kaoyan" or "考研" => "kaoyan",
            _ => string.IsNullOrWhiteSpace(c) ? "other" : c
        };
    }
}

public class WordPackEntry : ICreationTime
{
    public Guid Id { get; private set; }
    public Guid WordPackId { get; private set; }
    public WordPack? WordPack { get; private set; }
    public string Word { get; private set; } = string.Empty;
    public string? Phonetic { get; private set; }
    public string? Definition { get; private set; }
    public string? Example { get; private set; }
    public int Rank { get; private set; }
    public DateTime CreationTime { get; private set; }

    private WordPackEntry() { }

    public WordPackEntry(
        Guid wordPackId,
        string word,
        string? definition = null,
        string? example = null,
        string? phonetic = null,
        int rank = 0)
    {
        Id = Guid.NewGuid();
        WordPackId = wordPackId;
        Word = NormalizeWord(word);
        Definition = TrimOrNull(definition, 1000);
        Example = TrimOrNull(example, 2000);
        Phonetic = TrimOrNull(phonetic, 100);
        Rank = rank;
        CreationTime = DateTime.Now;
    }

    public void Update(string? definition, string? example, string? phonetic, int? rank)
    {
        if (definition != null)
            Definition = TrimOrNull(definition, 1000);
        if (example != null)
            Example = TrimOrNull(example, 2000);
        if (phonetic != null)
            Phonetic = TrimOrNull(phonetic, 100);
        if (rank.HasValue)
            Rank = rank.Value;
    }

    public static string NormalizeWord(string word) => (word ?? string.Empty).Trim();

    private static string? TrimOrNull(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var t = value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }
}

/// <summary>
/// 用户领取官方词本的记录（对应生成的个人单词本）。
/// </summary>
public class UserWordPackClaim : ICreationTime
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WordPackId { get; private set; }
    public Guid UserWordBookId { get; private set; }
    public int ClaimedCount { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? LastSyncAt { get; private set; }

    private UserWordPackClaim() { }

    public UserWordPackClaim(Guid userId, Guid wordPackId, Guid userWordBookId, int claimedCount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        WordPackId = wordPackId;
        UserWordBookId = userWordBookId;
        ClaimedCount = claimedCount;
        CreationTime = DateTime.Now;
        LastSyncAt = CreationTime;
    }

    public void UpdateProgress(int claimedCount)
    {
        ClaimedCount = claimedCount;
        LastSyncAt = DateTime.Now;
    }
}
