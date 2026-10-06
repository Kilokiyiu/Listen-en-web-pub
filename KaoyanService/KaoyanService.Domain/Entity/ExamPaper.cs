using DomainCommons;

namespace KaoyanService.Domain.Entity;

/// <summary>
/// 考研英语历年试卷（英语一 / 英语二）。
/// </summary>
public class ExamPaper : IEntity, ICreationTime
{
    public const string SeriesEnglish1 = "eng1";
    public const string SeriesEnglish2 = "eng2";

    public Guid Id { get; private set; }
    public int Year { get; private set; }

    /// <summary>eng1 = 英语一，eng2 = 英语二</summary>
    public string Series { get; private set; } = SeriesEnglish1;

    public string Title { get; private set; } = string.Empty;
    public bool IsVisible { get; private set; }
    public int SequenceNumber { get; private set; }
    public DateTime CreationTime { get; init; }

    private ExamPaper() { }

    public ExamPaper(int year, string series, string title, int sequenceNumber)
    {
        Id = Guid.NewGuid();
        Year = year;
        Series = NormalizeSeries(series);
        Title = title?.Trim() ?? string.Empty;
        SequenceNumber = sequenceNumber;
        IsVisible = true;
        CreationTime = DateTime.Now;
    }

    public static string NormalizeSeries(string series)
    {
        var s = (series ?? string.Empty).Trim().ToLowerInvariant();
        return s is SeriesEnglish1 or "英语一" or "english1" ? SeriesEnglish1 : SeriesEnglish2;
    }

    public static string SeriesDisplayName(string series) =>
        NormalizeSeries(series) == SeriesEnglish1 ? "英语一" : "英语二";

    public ExamPaper ChangeTitle(string title)
    {
        Title = title?.Trim() ?? string.Empty;
        return this;
    }

    public ExamPaper Show()
    {
        IsVisible = true;
        return this;
    }

    public ExamPaper Hide()
    {
        IsVisible = false;
        return this;
    }
}
