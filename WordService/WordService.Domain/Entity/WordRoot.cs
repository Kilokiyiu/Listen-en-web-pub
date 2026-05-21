using DomainCommons;

namespace WordService.Domain.Entity;

public class WordRoot : ICreationTime
{
    public Guid Id { get; private set; }
    public int RootId { get; private set; } // 来自 word-root-workshop 的原始 ID
    public string Root { get; private set; }
    public string Origin { get; private set; } // Greek/Latin
    public string Meaning { get; private set; }
    public string? MeaningEn { get; private set; }
    public string Description { get; private set; }
    public DateTime CreationTime { get; private set; }

    public ICollection<WordRootExample> Examples { get; private set; } = new List<WordRootExample>();
    public ICollection<WordRootQuiz> Quizzes { get; private set; } = new List<WordRootQuiz>();

    private WordRoot() { }

    public WordRoot(int rootId, string root, string origin, string meaning, string? meaningEn, string description)
    {
        Id = Guid.NewGuid();
        RootId = rootId;
        Root = root;
        Origin = origin;
        Meaning = meaning;
        MeaningEn = meaningEn;
        Description = description;
        CreationTime = DateTime.Now;
    }
}

public class WordRootExample
{
    public Guid Id { get; private set; }
    public Guid WordRootId { get; private set; }
    public WordRoot WordRoot { get; private set; }
    public string Word { get; private set; }
    public string? Prefix { get; private set; }
    public string? Root { get; private set; }
    public string? Suffix { get; private set; }
    public string Meaning { get; private set; }
    public string? Explanation { get; private set; }

    private WordRootExample() { }

    public WordRootExample(Guid wordRootId, string word, string? prefix, string? root, string? suffix,
        string meaning, string? explanation)
    {
        Id = Guid.NewGuid();
        WordRootId = wordRootId;
        Word = word;
        Prefix = prefix;
        Root = root;
        Suffix = suffix;
        Meaning = meaning;
        Explanation = explanation;
    }
}

public class WordRootQuiz
{
    public Guid Id { get; private set; }
    public Guid WordRootId { get; private set; }
    public WordRoot WordRoot { get; private set; }
    public string Question { get; private set; }
    public string OptionsJson { get; private set; } // JSON 存储选项数组
    public int CorrectAnswer { get; private set; }

    public string[] GetOptions() => System.Text.Json.JsonSerializer.Deserialize<string[]>(OptionsJson) ?? Array.Empty<string>();

    private WordRootQuiz() { }

    public WordRootQuiz(Guid wordRootId, string question, string[] options, int correctAnswer)
    {
        Id = Guid.NewGuid();
        WordRootId = wordRootId;
        Question = question;
        OptionsJson = System.Text.Json.JsonSerializer.Serialize(options);
        CorrectAnswer = correctAnswer;
    }
}
