using DomainCommons;

namespace WordService.Domain.Entity;

public class UserWordRootProgress : ICreationTime

{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WordRootId { get; private set; }
    public WordRoot WordRoot { get; private set; }
    public bool IsMastered { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime? MasteredTime { get; private set; }

    private UserWordRootProgress() { }

    public UserWordRootProgress(Guid userId, Guid wordRootId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        WordRootId = wordRootId;
        IsMastered = false;
        CreationTime = DateTime.Now;
    }

    public void MarkAsMastered()
    {
        IsMastered = true;
        MasteredTime = DateTime.Now;
    }
}

/// <summary>
/// 用户自定义单词
/// </summary>
public class UserWord : ICreationTime
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Word { get; private set; }
    public string? Definition { get; private set; }
    public string? Example { get; private set; }
    public DateTime CreationTime { get; private set; }

    // SM-2 算法字段
    public int RepetitionCount { get; private set; } // 已复习次数
    public double EaseFactor { get; private set; } // 难度因子 (默认 2.5)
    public int Interval { get; private set; } // 间隔天数
    public DateTime? NextReview { get; private set; } // 下次复习时间

    private UserWord() { }

    public UserWord(Guid userId, string word, string? definition = null, string? example = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Word = word;
        Definition = definition;
        Example = example;
        CreationTime = DateTime.Now;
        RepetitionCount = 0;
        EaseFactor = 2.5;
        Interval = 0;
        NextReview = null;
    }

    /// <summary>
    /// 使用 SM-2 算法更新复习状态
    /// </summary>
    /// <param name="quality">评分 0-5, 0=完全忘记, 5=轻松记住</param>
    public void UpdateReview(int quality)
    {
        if (quality < 0 || quality > 5) return;

        if (quality >= 3) // 复习成功
        {
            if (RepetitionCount == 0)
                Interval = 1;
            else if (RepetitionCount == 1)
                Interval = 6;
            else
                Interval = (int)Math.Round(Interval * EaseFactor);

            RepetitionCount++;
        }
        else // 复习失败，重新开始
        {
            RepetitionCount = 0;
            Interval = 1;
        }

        // 更新难度因子
        EaseFactor = EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        if (EaseFactor < 1.3) EaseFactor = 1.3;

        NextReview = DateTime.Now.AddDays(Interval);
    }

    public void UpdateContent(string? definition, string? example)
    {
        Definition = definition;
        Example = example;
    }
}

/// <summary>
/// 单词复习记录
/// </summary>
public class WordReviewLog : ICreationTime
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WordId { get; private set; }
    public int Quality { get; private set; } // 评分 0-5
    public DateTime CreationTime { get; private set; }

    private WordReviewLog() { }

    public WordReviewLog(Guid userId, Guid wordId, int quality)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        WordId = wordId;
        Quality = quality;
        CreationTime = DateTime.Now;
    }
}
