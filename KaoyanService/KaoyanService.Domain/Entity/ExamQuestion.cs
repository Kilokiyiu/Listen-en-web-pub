using System.Text.Json;
using DomainCommons;

namespace KaoyanService.Domain.Entity;

/// <summary>
/// 考研单选题（完形空格或阅读题）。
/// </summary>
public class ExamQuestion : IEntity, ICreationTime
{
    public Guid Id { get; private set; }
    public Guid SectionId { get; private set; }
    public int Number { get; private set; }
    public string Stem { get; private set; } = string.Empty;
    public string OptionsJson { get; private set; } = "[]";
    public int CorrectAnswer { get; private set; }
    public string? Explanation { get; private set; }
    public int SequenceNumber { get; private set; }
    public DateTime CreationTime { get; init; }

    private ExamQuestion() { }

    public ExamQuestion(
        Guid sectionId,
        int number,
        string stem,
        string[] options,
        int correctAnswer,
        int sequenceNumber,
        string? explanation = null)
    {
        if (options == null || options.Length < 2)
            throw new ArgumentException("至少需要 2 个选项", nameof(options));
        if (correctAnswer < 0 || correctAnswer >= options.Length)
            throw new ArgumentOutOfRangeException(nameof(correctAnswer));

        Id = Guid.NewGuid();
        SectionId = sectionId;
        Number = number;
        Stem = stem?.Trim() ?? string.Empty;
        OptionsJson = JsonSerializer.Serialize(options);
        CorrectAnswer = correctAnswer;
        Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation.Trim();
        SequenceNumber = sequenceNumber;
        CreationTime = DateTime.Now;
    }

    public string[] GetOptions() =>
        JsonSerializer.Deserialize<string[]>(OptionsJson) ?? Array.Empty<string>();

    public ExamQuestion Update(
        int number,
        string stem,
        string[] options,
        int correctAnswer,
        int sequenceNumber,
        string? explanation)
    {
        if (options == null || options.Length < 2)
            throw new ArgumentException("至少需要 2 个选项", nameof(options));
        if (correctAnswer < 0 || correctAnswer >= options.Length)
            throw new ArgumentOutOfRangeException(nameof(correctAnswer));

        Number = number;
        Stem = stem?.Trim() ?? string.Empty;
        OptionsJson = JsonSerializer.Serialize(options);
        CorrectAnswer = correctAnswer;
        Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation.Trim();
        SequenceNumber = sequenceNumber;
        return this;
    }
}
