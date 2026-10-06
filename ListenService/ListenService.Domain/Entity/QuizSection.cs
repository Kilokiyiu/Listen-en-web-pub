using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 听力做题材料（挂在 Section A/B/C 下的一段原文 + 题目），如 News Report 1。
/// </summary>
public class QuizSection : IEntity, ICreationTime
{
    public static readonly string[] StandardGroups = ["Section A", "Section B", "Section C"];

    public Guid Id { get; private set; }
    public Guid AlbumId { get; private set; }

    /// <summary>大题分区：Section A / Section B / Section C</summary>
    public string GroupName { get; private set; } = "Section A";

    /// <summary>本段材料标题，如 News Report 1</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>本段听力原文（纯文本）</summary>
    public string Transcript { get; private set; } = string.Empty;

    /// <summary>本段音频相对路径，如 /audios/CET4/.../section-a.mp3</summary>
    public string? AudioUrl { get; private set; }

    public double DurationInSecond { get; private set; }
    public int SequenceNumber { get; private set; }
    public bool IsVisible { get; private set; }
    public DateTime CreationTime { get; init; }

    private QuizSection() { }

    public QuizSection(
        Guid albumId,
        string title,
        string transcript,
        string? audioUrl,
        int sequenceNumber,
        string? groupName = null)
    {
        Id = Guid.NewGuid();
        AlbumId = albumId;
        GroupName = NormalizeGroupName(groupName);
        Title = string.IsNullOrWhiteSpace(title) ? "Passage 1" : title.Trim();
        Transcript = transcript ?? string.Empty;
        AudioUrl = string.IsNullOrWhiteSpace(audioUrl) ? null : audioUrl.Trim();
        SequenceNumber = sequenceNumber;
        IsVisible = true;
        CreationTime = DateTime.Now;
    }

    public static string NormalizeGroupName(string? groupName)
    {
        var raw = (groupName ?? string.Empty).Trim();
        if (raw.Length == 0)
            return "Section A";

        foreach (var g in StandardGroups)
        {
            if (string.Equals(raw, g, StringComparison.OrdinalIgnoreCase))
                return g;
        }

        // 兼容 "A" / "section a" / "SectionA"
        var compact = raw.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        if (compact is "A" or "SECTIONA") return "Section A";
        if (compact is "B" or "SECTIONB") return "Section B";
        if (compact is "C" or "SECTIONC") return "Section C";
        return "Section A";
    }

    /// <summary>当 groupName 缺失时，按材料标题启发式推断 Section。</summary>
    public static string InferGroupName(string? groupName, string? title)
    {
        if (!string.IsNullOrWhiteSpace(groupName))
            return NormalizeGroupName(groupName);

        var t = title ?? string.Empty;
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"section\s*([abc])", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            var m = System.Text.RegularExpressions.Regex.Match(t, @"section\s*([abc])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return NormalizeGroupName($"Section {m.Groups[1].Value.ToUpperInvariant()}");
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"news\s*report|short\s*news", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "Section A";
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"long\s*conversation|conversation|dialogue", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "Section B";
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"passage|lecture|talk", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "Section C";
        return "Section A";
    }

    public QuizSection Update(string title, string transcript, int sequenceNumber, string? groupName = null)
    {
        Title = string.IsNullOrWhiteSpace(title) ? Title : title.Trim();
        Transcript = transcript ?? string.Empty;
        SequenceNumber = sequenceNumber;
        if (groupName != null)
            GroupName = NormalizeGroupName(groupName);
        return this;
    }

    public QuizSection SetGroupName(string? groupName)
    {
        GroupName = NormalizeGroupName(groupName);
        return this;
    }

    public QuizSection SetAudio(string? audioUrl, double durationInSecond = 0)
    {
        AudioUrl = string.IsNullOrWhiteSpace(audioUrl) ? null : audioUrl.Trim();
        DurationInSecond = durationInSecond;
        return this;
    }

    public QuizSection Show()
    {
        IsVisible = true;
        return this;
    }

    public QuizSection Hide()
    {
        IsVisible = false;
        return this;
    }
}
