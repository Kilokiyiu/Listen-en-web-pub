using DomainCommons;

namespace WordService.Domain.Entity;

/// <summary>
/// 用户分享的单词本快照（分享码领取，不随原词本后续改动）。
/// </summary>
public class SharedWordBook : ICreationTime
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public Guid OwnerUserId { get; private set; }
    public Guid SourceWordBookId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime CreationTime { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private readonly List<SharedWordBookEntry> _entries = new();
    public IReadOnlyCollection<SharedWordBookEntry> Entries => _entries;

    private SharedWordBook() { }

    public SharedWordBook(
        Guid ownerUserId,
        Guid sourceWordBookId,
        string title,
        string code,
        DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        SourceWordBookId = sourceWordBookId;
        Title = string.IsNullOrWhiteSpace(title) ? "共享单词本" : title.Trim();
        if (Title.Length > 100)
            Title = Title[..100];
        Code = NormalizeCode(code);
        CreationTime = DateTime.Now;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public void Revoke() => IsRevoked = true;

    public bool IsAvailable(DateTime now) =>
        !IsRevoked && ExpiresAt > now;

    public static string NormalizeCode(string code) =>
        (code ?? string.Empty).Trim().ToUpperInvariant();
}

public class SharedWordBookEntry : ICreationTime
{
    public Guid Id { get; private set; }
    public Guid SharedWordBookId { get; private set; }
    public SharedWordBook? SharedWordBook { get; private set; }
    public string Word { get; private set; } = string.Empty;
    public string? Definition { get; private set; }
    public string? Example { get; private set; }
    public int Rank { get; private set; }
    public DateTime CreationTime { get; private set; }

    private SharedWordBookEntry() { }

    public SharedWordBookEntry(
        Guid sharedWordBookId,
        string word,
        string? definition = null,
        string? example = null,
        int rank = 0)
    {
        Id = Guid.NewGuid();
        SharedWordBookId = sharedWordBookId;
        Word = (word ?? string.Empty).Trim();
        Definition = TrimOrNull(definition, 1000);
        Example = TrimOrNull(example, 2000);
        Rank = rank;
        CreationTime = DateTime.Now;
    }

    private static string? TrimOrNull(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var t = value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }
}
