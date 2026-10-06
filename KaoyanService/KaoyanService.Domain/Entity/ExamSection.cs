using DomainCommons;

namespace KaoyanService.Domain.Entity;

/// <summary>
/// 试卷分区：完形填空 / 阅读理解。
/// </summary>
public class ExamSection : IEntity, ICreationTime
{
    public const string TypeCloze = "cloze";
    public const string TypeReading = "reading";

    public Guid Id { get; private set; }
    public Guid PaperId { get; private set; }

    /// <summary>cloze | reading</summary>
    public string SectionType { get; private set; } = TypeCloze;

    public string Title { get; private set; } = string.Empty;

    /// <summary>文章正文；完形带空格标记，阅读为篇章文本</summary>
    public string Passage { get; private set; } = string.Empty;

    public int SequenceNumber { get; private set; }
    public DateTime CreationTime { get; init; }

    private ExamSection() { }

    public ExamSection(Guid paperId, string sectionType, string title, string passage, int sequenceNumber)
    {
        Id = Guid.NewGuid();
        PaperId = paperId;
        SectionType = NormalizeType(sectionType);
        Title = title?.Trim() ?? string.Empty;
        Passage = passage ?? string.Empty;
        SequenceNumber = sequenceNumber;
        CreationTime = DateTime.Now;
    }

    public static string NormalizeType(string type)
    {
        var t = (type ?? string.Empty).Trim().ToLowerInvariant();
        return t is TypeReading or "阅读" or "阅读理解" ? TypeReading : TypeCloze;
    }

    public ExamSection Update(string title, string passage, int sequenceNumber)
    {
        Title = title?.Trim() ?? string.Empty;
        Passage = passage ?? string.Empty;
        SequenceNumber = sequenceNumber;
        return this;
    }
}
